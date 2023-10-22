// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.InvisibleNodes
{
    public class InvisibleNodeLocator : IInvisibleNodeLocator
    {
        private readonly IInvisibleNodeRulesManager _rulesManager;

        public InvisibleNodeLocator(IInvisibleNodeRulesManager rulesManager)
        {
            _rulesManager = rulesManager;
        }
    
        /// <inheritdoc />
        public IPublishedContent Locate(IPublishedContent node, string path, string culture)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (string.IsNullOrEmpty(path))
                return null;

            string[] segments = path.Split('/');
        
            if (segments.Length == 0)
                return null;

            return WalkContentTree(node, segments, culture);
        }

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

                if (child.IsInvisibleNode(_rulesManager))
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