// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Our.Umbraco.VirtualNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.VirtualNodes;

public class VirtualNodeUrlProvider : IUrlProvider
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ISiteDomainMapper _siteDomainMapper;
    private readonly IVirtualNodeRulesManager _rulesManager;

    public VirtualNodeUrlProvider(
        IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        ISiteDomainMapper siteDomainMapper, 
        IVirtualNodeRulesManager rulesManager)
    {
        _siteDomainMapper = siteDomainMapper;
        _variationContextAccessor = variationContextAccessor;
        _umbracoContextAccessor = umbracoContextAccessor;
        _rulesManager = rulesManager;
    }

    /// <inheritdoc />
    public UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current)
    {
        string route = GenerateRoute(content, culture);

        if (mode == UrlMode.Auto || mode == UrlMode.Default || mode == UrlMode.Relative)
            return UrlInfo.Url(route, culture);

        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) || umbracoContext.Domains is null)
            return null;

        var domainCache = umbracoContext.Domains;

        var domainAndUris = domainCache.GetAll(false)
            .Select(domain => new DomainAndUri(domain, current))
            .ToList();

        var mappedDomain = _siteDomainMapper.MapDomain(domainAndUris, current, culture, domainCache.DefaultCulture);

        if (mappedDomain is null)
            return UrlInfo.Url(route, culture);

        var builder = new UriBuilder(mappedDomain.Uri)
        {
            Path = WebPath.Combine(mappedDomain.Uri.AbsolutePath, route)
        };

        return UrlInfo.Url(builder.ToString(), culture);
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

        var domainAndUris = domainCache.GetAll(false)
            .Select(domain => new DomainAndUri(domain, current))
            .ToList();

        var mappedDomains =
            _siteDomainMapper.MapDomains(domainAndUris, current, true, null, domainCache.DefaultCulture);

        var urls = new List<UrlInfo>();

        foreach (var mappedDomain in mappedDomains)
        {
            var builder = new UriBuilder(mappedDomain.Uri)
            {
                Path = WebPath.Combine(mappedDomain.Uri.AbsolutePath, route)
            };

            urls.Add(UrlInfo.Url(builder.ToString()));
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
            .Where(ancestor => !ancestor.IsVirtualNode(_rulesManager) && ancestor.Level > 1)
            .Select(ancestor => ancestor.UrlSegment(_variationContextAccessor, culture))
            .Reverse()
            .ToList();

        return string.Join('/', segments)
            .EnsureStartsWith('/')
            .EnsureEndsWith('/');
    }
}