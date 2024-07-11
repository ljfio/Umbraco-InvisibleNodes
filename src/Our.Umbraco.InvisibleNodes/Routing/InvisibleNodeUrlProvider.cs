// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
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

    public InvisibleNodeUrlProvider(IUmbracoContextAccessor umbracoContextAccessor,
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
        // Get the matching domain and generated route
        string route = GenerateRoute(content, culture);

        var matchingDomain = GetMatchingDomain(current, culture);

        if (matchingDomain is null)
        {
            var currentAuthority = new Uri(current.GetLeftPart(UriPartial.Authority));
            return ToUrlInfo(currentAuthority, route, culture, mode);
        }

        // Combine the matching domain URI with path
        var baseUri = new Uri(matchingDomain.Uri.GetLeftPart(UriPartial.Authority));
        string baseRoute = matchingDomain.Uri.AbsolutePath;

        string combinedRoute = CombinePaths(baseRoute, route)
            .EnsureStartsWith('/');

        return ToUrlInfo(baseUri, combinedRoute, culture, mode);
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

        var ancestors = content.Ancestors()
            .Select(a => a.Id)
            .ToArray();

        var domainCache = umbracoContext.Domains;

        var domainAndUris = domainCache.GetAll(true)
            .Where(domain => ancestors.Contains(domain.ContentId))
            .Select(domain => new DomainAndUri(domain, current))
            .ToList();

        var mappedDomains = _siteDomainMapper
            .MapDomains(domainAndUris, current, true, null, domainCache.DefaultCulture);

        var urls = new List<UrlInfo>();

        foreach (var mappedDomain in mappedDomains)
        {
            string route = GenerateRoute(content, mappedDomain.Culture);

            var baseUri = new Uri(mappedDomain.Uri.GetLeftPart(UriPartial.Authority));
            string baseRoute = mappedDomain.Uri.AbsolutePath;

            string combinedRoute = CombinePaths(baseRoute, route)
                .EnsureStartsWith('/');

            var url = ToUrlInfo(baseUri, combinedRoute, mappedDomain.Culture, UrlMode.Absolute);

            if (url is not null)
                urls.Add(url);
        }

        return urls;
    }

    /// <summary>
    /// Generates out the correct route based on the 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    private string GenerateRoute(IPublishedContent content, string? culture = null)
    {
        var segments = content.AncestorsOrSelf()
            .Where(ancestor => !ancestor.IsInvisibleNode(_rulesManager) && ancestor.Level > 1)
            .Select(ancestor => ancestor.UrlSegment(_variationContextAccessor, culture))
            .Reverse()
            .ToList();

        return string.Join('/', segments)
            .EnsureStartsWith("/");
    }

    /// <summary>
    /// Tries to locate the matching doamin
    /// </summary>
    /// <param name="current"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    private DomainAndUri? GetMatchingDomain(Uri current, string? culture)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) || umbracoContext.Domains is null)
            return null;

        // Locate the matching domain for the request
        var domainCache = umbracoContext.Domains;

        var domainAndUris = domainCache.GetAll(true)
            .Select(domain => new DomainAndUri(domain, current))
            .ToList();

        if (!domainAndUris.Any())
            return null;

        return _siteDomainMapper.MapDomain(domainAndUris, current, culture, domainCache.DefaultCulture);
    }

    /// <summary>
    /// Converts to a <see cref="UrlInfo"/>
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="route"></param>
    /// <param name="culture"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    private UrlInfo? ToUrlInfo(Uri uri, string route, string? culture, UrlMode mode)
    {
        string path = _requestHandlerOptions.Value.AddTrailingSlash ? route.EnsureEndsWith("/") : route;

        if (mode != UrlMode.Absolute)
            return UrlInfo.Url(path, culture);

        if (Uri.TryCreate(uri, path, out var combined))
            return UrlInfo.Url(combined.ToString(), culture);

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