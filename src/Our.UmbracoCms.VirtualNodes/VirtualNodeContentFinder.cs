// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Our.UmbracoCms.VirtualNodes.Core;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Our.UmbracoCms.VirtualNodes
{
    public class VirtualNodeContentFinder : IContentFinder
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IVirtualNodeCache _nodeCache;
        private readonly IVirtualNodeRulesManager _rulesManager;

        public VirtualNodeContentFinder(
            IUmbracoContextAccessor umbracoContextAccessor,
            IVirtualNodeCache nodeCache,
            IVirtualNodeRulesManager rulesManager)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _nodeCache = nodeCache;
            _rulesManager = rulesManager;
        }

        /// <inheritdoc />
        public bool TryFindContent(PublishedRequest request)
        {
            var context = _umbracoContextAccessor.UmbracoContext;

            if (context is null || context.Content == null)
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

            string[] segments = path.Split('/');

            if (root == null || segments.Length == 0)
            {
                return false;
            }

            var foundNode = WalkContentTree(root, segments, culture);

            if (foundNode != null)
            {
                _nodeCache.StoreRoute(host, path, foundNode.Id);
                request.PublishedContent = foundNode;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Walks the published content tree to locate a node that may be virtually hidden
        /// </summary>
        /// <param name="node"></param>
        /// <param name="segments"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        private IPublishedContent WalkContentTree(IPublishedContent node, string[] segments, string culture)
        {
            string segment = segments.First();

            if (segments.Length == 1 && string.Equals(node.UrlSegment(culture), segment))
            {
                return node;
            }

            string[] childSegments = segments.Skip(1).ToArray();

            foreach (var child in node.Children.EmptyNull())
            {
                if (string.Equals(child.UrlSegment(culture), segment))
                {
                    if (segments.Length == 1)
                        return child;

                    var grandChild = WalkContentTree(child, childSegments, culture);

                    if (grandChild != null)
                        return grandChild;
                }

                if (child.IsVirtualNode(_rulesManager))
                {
                    var hiddenChild = WalkContentTree(child, segments, culture);

                    if (hiddenChild != null)
                        return hiddenChild;
                }
            }

            return null;
        }
    }
}