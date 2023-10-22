// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Routing;

namespace Our.UmbracoCms.InvisibleNodes
{
    public class InvisibleNodeContentFinder : IContentFinder
    {
        private readonly IInvisibleNodeCache _nodeCache;
        private readonly IInvisibleNodeLocator _nodeLocator;

        public InvisibleNodeContentFinder(
            IInvisibleNodeCache nodeCache,
            IInvisibleNodeLocator nodeLocator)
        {
            _nodeCache = nodeCache;
            _nodeLocator = nodeLocator;
        }

        /// <inheritdoc />
        public bool TryFindContent(PublishedRequest request)
        {
            var context = request.UmbracoContext;

            if (context == null || context.Content == null)
                return false;

            string host = request.Uri.Host;
            string path = request.Uri.AbsolutePath.Trim('/');

            int? cached = _nodeCache.GetRoute(host, path);

            if (cached.HasValue)
            {
                var cachedContent = context.Content.GetById(cached.Value);

                if (cachedContent != null)
                {
                    request.PublishedContent = cachedContent;
                    return true;
                }

                _nodeCache.ClearRoute(host, path);
            }

            string culture = request.Culture.ToString();

            var root = request.Domain != null
                ? context.Content.GetById(request.Domain.ContentId)
                : context.Content.GetAtRoot(culture).FirstOrDefault();

            if (root == null)
                return false;

            var foundNode = _nodeLocator.Locate(root, path, culture);

            if (foundNode != null)
            {
                _nodeCache.StoreRoute(host, path, foundNode.Id);
                request.PublishedContent = foundNode;
                return true;
            }

            return false;
        }
    }
}