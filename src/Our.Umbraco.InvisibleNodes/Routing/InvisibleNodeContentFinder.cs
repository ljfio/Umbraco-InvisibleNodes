// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.InvisibleNodes.Routing;

public class InvisibleNodeContentFinder : IContentFinder
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IInvisibleNodeCache _invisibleNodeCache;
    private readonly IInvisibleNodeLocator _invisibleNodeLocator;

    public InvisibleNodeContentFinder(
        IUmbracoContextAccessor umbracoContextAccessor,
        IInvisibleNodeCache invisibleNodeCache,
        IInvisibleNodeLocator invisibleNodeLocator)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _invisibleNodeCache = invisibleNodeCache;
        _invisibleNodeLocator = invisibleNodeLocator;
    }

    /// <inheritdoc />
    public Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var context) || context.Content is null)
            return Task.FromResult(false);

        string host = request.Uri.GetLeftPart(UriPartial.Authority);
        string path = request.Uri.AbsolutePath;

        int? cached = _invisibleNodeCache.GetRoute(host, path);

        if (cached.HasValue)
        {
            var cachedContent = context.Content.GetById(cached.Value);

            if (cachedContent is not null)
            {
                request.SetPublishedContent(cachedContent);
                return Task.FromResult(true);
            }
            
            _invisibleNodeCache.ClearRoute(host, path);
        }
        
        string? culture = request.Culture;

        var root = request.Domain is not null
            ? context.Content.GetById(request.Domain.ContentId)
            : context.Content.GetAtRoot(culture).FirstOrDefault();

        if (root is null)
            return Task.FromResult(false);
        
        var foundNode = _invisibleNodeLocator.Locate(root, path, culture);

        if (foundNode is not null)
        {
            _invisibleNodeCache.StoreRoute(host, path, foundNode.Id);
            request.SetPublishedContent(foundNode);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}