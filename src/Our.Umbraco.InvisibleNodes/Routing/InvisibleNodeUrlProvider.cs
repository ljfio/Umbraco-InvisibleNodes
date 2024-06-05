// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Our.Umbraco.InvisibleNodes.Core;
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

    public InvisibleNodeUrlProvider(
        IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        ISiteDomainMapper siteDomainMapper,
        IInvisibleNodeRulesManager rulesManager)
    {
        _siteDomainMapper = siteDomainMapper;
        _variationContextAccessor = variationContextAccessor;
        _umbracoContextAccessor = umbracoContextAccessor;
        _rulesManager = rulesManager;
    }

    /// <inheritdoc />
    public UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) || umbracoContext.Domains is null)
            return null;

        // Get the current authority and generated route
        var currentAuthority = new Uri(current.GetLeftPart(UriPartial.Authority));
        string route = GenerateRoute(content, culture);

        // Locate the matching domain for the request
        var domainCache = umbracoContext.Domains;

        var domainAndUris = domainCache.GetAll(true)
            .Select(domain => new DomainAndUri(domain, currentAuthority))
            .ToList();

        if (!domainAndUris.Any())
            return Uri.TryCreate(currentAuthority, route, out var currentCombined)
                ? UrlInfo.Url(currentCombined.ToString(), culture)
                : null;

        var mappedDomain = _siteDomainMapper.MapDomain(domainAndUris, current, culture, domainCache.DefaultCulture);

        if (mappedDomain is null)
            return UrlInfo.Url(route, culture);

        // Combine the domain URI / path
        string path = WebPath.Combine(mappedDomain.Uri.AbsolutePath, route)
            .EnsureStartsWith('/')
            .EnsureEndsWith('/');

        if (mode == UrlMode.Auto || mode == UrlMode.Default || mode == UrlMode.Relative)
            return UrlInfo.Url(path, culture);

        if (!Uri.TryCreate(mappedDomain.Uri, path, out var uri))
            return null;

        return UrlInfo.Url(uri.AbsoluteUri, culture);
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

        string route = GenerateRoute(content);

        var domainCache = umbracoContext.Domains;

        var domainAndUris = domainCache.GetAssigned(id)
            .Select(domain => new DomainAndUri(domain, current))
            .ToList();

        var mappedDomains = _siteDomainMapper
            .MapDomains(domainAndUris, current, true, null, domainCache.DefaultCulture);

        var urls = new List<UrlInfo>();

        foreach (var mappedDomain in mappedDomains)
        {
            if (!Uri.TryCreate(mappedDomain.Uri, route, out var uri))
                continue;

            urls.Add(UrlInfo.Url(uri.ToString()));
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
            .EnsureStartsWith('/')
            .EnsureEndsWith('/');
    }
}