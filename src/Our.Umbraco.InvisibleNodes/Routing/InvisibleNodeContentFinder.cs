// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
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
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IInvisibleNodeRulesManager _rulesManager;
    private readonly IOptionsMonitor<InvisibleNodeSettings> _settings;

    public InvisibleNodeContentFinder(
        IUmbracoContextAccessor umbracoContextAccessor,
        IInvisibleNodeCache invisibleNodeCache,
        IInvisibleNodeLocator invisibleNodeLocator,
        IDocumentNavigationQueryService navigationQueryService,
        IDocumentUrlService documentUrlService,
        IPublishedUrlProvider publishedUrlProvider,
        IInvisibleNodeRulesManager rulesManager,
        IOptionsMonitor<InvisibleNodeSettings> settings)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _invisibleNodeCache = invisibleNodeCache;
        _invisibleNodeLocator = invisibleNodeLocator;
        _navigationQueryService = navigationQueryService;
        _documentUrlService = documentUrlService;
        _publishedUrlProvider = publishedUrlProvider;
        _rulesManager = rulesManager;
        _settings = settings;
    }

    /// <inheritdoc />
    public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var context))
            return false;

        string host = request.Uri.GetLeftPart(UriPartial.Authority);
        string path = request.Uri.AbsolutePath;

        string culture = request.Culture ?? context.Domains.DefaultCulture;

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Routes are cached by host and full path, but a hostname can include a path (e.g. example.org/en) and be
        // assigned below the home node, so only the segments after the hostname's path belong to its content
        int domainLength = request.Domain is null
            ? 0
            : Math.Min(
                request.Domain.Uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length,
                segments.Length);

        // Check the cache first, starting with the full path and then working up through its ancestors
        for (int length = segments.Length; length > domainLength; length--)
        {
            string cachedPath = string.Join('/', segments[..length]).EnsureStartsWith('/');

            int? cached = _invisibleNodeCache.GetRoute(host, cachedPath);

            if (!cached.HasValue)
                continue;

            var cachedContent = context.Content.GetById(cached.Value);

            if (cachedContent is null)
            {
                _invisibleNodeCache.ClearRoute(host, cachedPath);
                continue;
            }

            // The full path is cached
            if (length == segments.Length)
            {
                request.SetPublishedContent(cachedContent);
                return true;
            }

            // An ancestor is cached, so only walk the tree below it
            string remainingPath = string.Join('/', segments[length..]);

            var descendant = await _invisibleNodeLocator.Locate(context.Content, cachedContent, remainingPath, culture);

            if (descendant is not null)
            {
                _invisibleNodeCache.StoreRoute(host, path, descendant.Id);
                request.SetPublishedContent(descendant);
                return true;
            }

            // Not below the nearest cached ancestor, fall back to walking from the root
            break;
        }

        // Locate the root for the request
        var root = await LocateRootNode(context.Content, request.Domain, culture);
        
        if (root is null)
            return false;
        
        // Find the matching node
        string relativePath = string.Join('/', segments[domainLength..]);

        var foundNode = await _invisibleNodeLocator.Locate(context.Content, root, relativePath, culture);

        if (foundNode is null)
            return HandleNativeUrl(request, context, root, relativePath);
        
        _invisibleNodeCache.StoreRoute(host, path, foundNode.Id);
        request.SetPublishedContent(foundNode);
        return true;
    }

    /// <summary>
    /// Handles a request for the URL a node would have without this package, i.e. one that includes the segment
    /// of an invisible node, which Umbraco would otherwise go on to serve
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="root"></param>
    /// <param name="relativePath">path relative to the <paramref name="root"/></param>
    /// <returns>true when the request has been handled and Umbraco should not serve the content</returns>
    private bool HandleNativeUrl(
        IPublishedRequestBuilder request,
        IUmbracoContext context,
        IPublishedContent root,
        string relativePath)
    {
        var behavior = _settings.CurrentValue.NativeUrls;

        if (behavior == NativeUrlBehavior.Allow)
            return false;

        string route = Uri.UnescapeDataString(relativePath).EnsureStartsWith('/');

        var key = _documentUrlService.GetDocumentKeyByRoute(
            route,
            request.Culture,
            request.Domain?.ContentId,
            context.InPreviewMode);

        if (!key.HasValue)
            return false;

        var node = context.Content.GetById(context.InPreviewMode, key.Value);

        if (node is null || !HasInvisibleSegment(context.Content, node, root))
            return false;

        // The invisible node itself does not have a URL to redirect to
        if (behavior == NativeUrlBehavior.Redirect && !node.IsInvisibleNode(_rulesManager))
        {
            string url = _publishedUrlProvider.GetUrl(node, UrlMode.Default, request.Culture, request.Uri);

            if (!string.IsNullOrEmpty(url) && !url.StartsWith('#'))
            {
                request.SetRedirectPermanent(url + request.Uri.Query);
                return true;
            }
        }

        // Handled without any content, so the request is not found
        return true;
    }

    /// <summary>
    /// Checks if the native URL of the node includes the segment of an invisible node
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="node"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    private bool HasInvisibleSegment(IPublishedContentCache cache, IPublishedContent node, IPublishedContent root)
    {
        if (node.Level > root.Level && node.IsInvisibleNode(_rulesManager))
            return true;

        if (!_navigationQueryService.TryGetAncestorsKeys(node.Key, out var ancestorKeys))
            return false;

        foreach (var ancestorKey in ancestorKeys)
        {
            var ancestor = cache.GetById(ancestorKey);

            if (ancestor is not null && ancestor.Level > root.Level && ancestor.IsInvisibleNode(_rulesManager))
                return true;
        }

        return false;
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