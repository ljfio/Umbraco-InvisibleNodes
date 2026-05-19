// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Routing;

public class InvisibleNodeContentFinder : IContentFinder
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IInvisibleNodeCache _invisibleNodeCache;
    private readonly IInvisibleNodeLocator _invisibleNodeLocator;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    
    public InvisibleNodeContentFinder(
        IUmbracoContextAccessor umbracoContextAccessor,
        IInvisibleNodeCache invisibleNodeCache,
        IInvisibleNodeLocator invisibleNodeLocator,
        IDocumentNavigationQueryService navigationQueryService)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _invisibleNodeCache = invisibleNodeCache;
        _invisibleNodeLocator = invisibleNodeLocator;
        _navigationQueryService = navigationQueryService;
    }

    /// <inheritdoc />
    public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var context))
            return false;

        string host = request.Uri.GetLeftPart(UriPartial.Authority);
        string path = request.Uri.AbsolutePath;

        // Check the cache first
        int? cached = _invisibleNodeCache.GetRoute(host, path);

        if (cached.HasValue)
        {
            var cachedContent = context.Content.GetById(cached.Value);

            if (cachedContent is not null)
            {
                request.SetPublishedContent(cachedContent);
                return true;
            }
            
            _invisibleNodeCache.ClearRoute(host, path);
        }
        
        // Locate the root for the request
        string culture = request.Culture ?? context.Domains.DefaultCulture;

        var root = await LocateRootNode(context.Content, request.Domain, culture);
        
        if (root is null)
            return false;
        
        // Find the matching node
        var foundNode = await _invisibleNodeLocator.Locate(context.Content, root, path, culture);

        if (foundNode is null)
            return false;
        
        _invisibleNodeCache.StoreRoute(host, path, foundNode.Id);
        request.SetPublishedContent(foundNode);
        return true;
    }

    private async Task<IPublishedContent?> LocateRootNode(
        IPublishedContentCache cache,
        DomainAndUri? domain,
        string? culture)
    {
        if (domain is not null)
            return await cache.GetByIdAsync(domain.ContentId);

        if (!_navigationQueryService.TryGetRootKeys(out var keys))
            return null;

        IPublishedContent? fallback = null;

        foreach (var key in keys)
        {
            var root = await cache.GetByIdAsync(key);

            if (root is null)
                continue;

            if (root.HasCulture(culture))
                return root;

            fallback ??= root;
        }

        return fallback;
    }
}