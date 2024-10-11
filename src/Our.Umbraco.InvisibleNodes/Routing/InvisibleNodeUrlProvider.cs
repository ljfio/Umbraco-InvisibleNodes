// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Routing;

public class InvisibleNodeUrlProvider : IUrlProvider
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ISiteDomainMapper _siteDomainMapper;
    private readonly IInvisibleNodeRulesManager _rulesManager;
    private readonly IOptions<RequestHandlerSettings> _requestHandlerOptions;

    public InvisibleNodeUrlProvider(
        IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        ISiteDomainMapper siteDomainMapper,
        IInvisibleNodeRulesManager rulesManager,
        IOptions<RequestHandlerSettings> requestHandlerOptions)
    {
        _siteDomainMapper = siteDomainMapper;
        _variationContextAccessor = variationContextAccessor;
        _umbracoContextAccessor = umbracoContextAccessor;
        _rulesManager = rulesManager;
        _requestHandlerOptions = requestHandlerOptions;
    }

    /// <inheritdoc />
    public UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) ||
            umbracoContext.Domains is null ||
            umbracoContext.Content is null)
            return null;

        // Locate the matching domain for the request
        var domainCache = umbracoContext.Domains;
        string defaultCulture = domainCache.DefaultCulture;

        // Get the matching domain and generated route
        var matchingDomain = GetMatchingDomain(domainCache, content, current, culture);

        if (matchingDomain is not null ||
            string.IsNullOrEmpty(culture) ||
            Equals(culture, defaultCulture))
        {
            // Locate the root for the domain
            var root = matchingDomain is not null
                ? umbracoContext.Content.GetById(matchingDomain.ContentId)
                : null;

            var baseUri = matchingDomain is not null
                ? matchingDomain.Uri
                : new Uri(current.GetLeftPart(UriPartial.Authority));

            string route = GenerateRoute(content, root, culture, matchingDomain?.Culture);

            var combinedUri = CombineUri(baseUri, route);

            if (combinedUri is null)
                return null;

            return ToUrlInfo(combinedUri, mode, culture, current);
        }

        return null;
    }

    /// <inheritdoc />
    public IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) ||
            umbracoContext.Content is null ||
            umbracoContext.Domains is null)
            return Enumerable.Empty<UrlInfo>();

        var content = umbracoContext.Content.GetById(id);

        if (content is null)
            return Enumerable.Empty<UrlInfo>();

        var domainCache = umbracoContext.Domains;

        var mappedDomains = GetMatchingDomains(domainCache, content, current);

        var urls = new List<UrlInfo>();

        foreach (var mappedDomain in mappedDomains)
        {
            var root = umbracoContext.Content.GetById(mappedDomain.ContentId);

            string route = GenerateRoute(content, root, mappedDomain.Culture, domainCache.DefaultCulture);

            var uri = CombineUri(mappedDomain.Uri, route);

            if (uri is null)
                continue;

            var url = ToUrlInfo(uri, UrlMode.Absolute, mappedDomain.Culture, mappedDomain.Uri);

            urls.Add(url);
        }

        return urls;
    }

    /// <summary>
    /// Generates out the correct route based on the <see cref="InvisibleNodeRulesManager"/>
    /// </summary>
    /// <param name="content"></param>
    /// <param name="root"></param>
    /// <param name="culture"></param>
    /// <param name="expectedCulture"></param>
    /// <returns></returns>
    private string GenerateRoute(
        IPublishedContent content,
        IPublishedContent? root,
        string? culture,
        string? expectedCulture)
    {
        var segments = content.AncestorsOrSelf()
            .Where(ancestor => IsVisible(ancestor, root, culture, expectedCulture))
            .Select(ancestor => ancestor.UrlSegment(_variationContextAccessor, culture))
            .Reverse()
            .ToList();

        return string.Join('/', segments)
            .EnsureStartsWith("/");
    }

    /// <summary>
    /// Checks if the node is visible in the URL
    /// </summary>
    /// <param name="node"></param>
    /// <param name="root"></param>
    /// <param name="culture"></param>
    /// <param name="expectedCulture"></param>
    /// <returns></returns>
    private bool IsVisible(
        IPublishedContent node, 
        IPublishedContent? root,
        string? culture,
        string? expectedCulture)
    {
        int level = Equals(culture, expectedCulture) ? 1 : 0;

        return !node.IsInvisibleNode(_rulesManager) &&
               (root == null ? node.Level > level : node.Level > root.Level);
    }

    /// <summary>
    /// Tries to locate the matching domain for the content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="current"></param>
    /// <param name="culture"></param>
    /// <param name="domainCache"></param>
    /// <returns></returns>
    private DomainAndUri? GetMatchingDomain(
        IDomainCache domainCache,
        IPublishedContent content,
        Uri current,
        string? culture)
    {
        var domains = content.AncestorsOrSelf()
            .Select(node => domainCache.GetAssigned(node.Id, includeWildcards: false))
            .FirstOrDefault(domains => domains.Any());

        return DomainUtilities.SelectDomain(
            domains,
            current,
            culture,
            domainCache.DefaultCulture,
            _siteDomainMapper.MapDomain);
    }

    /// <summary>
    /// Tries to locate the matching domains for the content
    /// </summary>
    /// <param name="domainCache"></param>
    /// <param name="content"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private IEnumerable<DomainAndUri> GetMatchingDomains(
        IDomainCache domainCache,
        IPublishedContent content,
        Uri current)
    {
        var domainAndUris = content.AncestorsOrSelf()
            .SelectMany(node => domainCache.GetAssigned(node.Id, includeWildcards: false))
            .Select(domain => new DomainAndUri(domain, current))
            .ToArray();

        return _siteDomainMapper.MapDomains(domainAndUris, current, true, null, domainCache.DefaultCulture);
    }

    /// <summary>
    /// Converts to a <see cref="UrlInfo"/>
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="mode"></param>
    /// <param name="culture"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private UrlInfo ToUrlInfo(Uri uri, UrlMode mode, string? culture, Uri current)
    {
        var newMode = mode == UrlMode.Absolute || !Equals(uri.Authority, current.Authority)
            ? UrlMode.Absolute
            : UrlMode.Relative;
        
        if (newMode != UrlMode.Absolute)
            return UrlInfo.Url(uri.AbsolutePath, culture);

        return UrlInfo.Url(uri.ToString(), culture);
    }

    /// <summary>
    /// Combines the <paramref name="uri"/> and <paramref name="relativePath"/>
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    private Uri? CombineUri(Uri uri, string relativePath)
    {
        // Combine the absolute path and relative path
        string combinedPath = CombinePaths(uri.AbsolutePath, relativePath)
            .EnsureStartsWith('/');

        // Ensure ends with trailing slash if configured
        string path = _requestHandlerOptions.Value.AddTrailingSlash
            ? combinedPath.EnsureEndsWith("/")
            : combinedPath;

        // Get the authority for the new Uri
        var authority = new Uri(uri.GetLeftPart(UriPartial.Authority));

        if (Uri.TryCreate(authority, path, out var absolute))
            return absolute;
        
        if (Uri.TryCreate(path, UriKind.Relative, out var relative))
            return relative;

        return null;
    }

    /// <summary>
    /// Combine two paths with the <paramref name="separator"/>
    /// </summary>
    /// <param name="first">first path</param>
    /// <param name="second">second path</param>
    /// <param name="separator">separator</param>
    /// <returns></returns>
    private string CombinePaths(string first, string second, char separator = '/') =>
        string.Join(separator, first.Trim(separator), second.Trim(separator));
}