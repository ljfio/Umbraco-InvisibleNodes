// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes;

public class InvisibleNodeLocator : IInvisibleNodeLocator
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IInvisibleNodeRulesManager _rulesManager;

    public InvisibleNodeLocator(
        IVariationContextAccessor variationContextAccessor,
        IInvisibleNodeRulesManager rulesManager)
    {
        _variationContextAccessor = variationContextAccessor;
        _rulesManager = rulesManager;
    }
    
    /// <inheritdoc />
    public IPublishedContent? Locate(IPublishedContent? node, string? path, string? culture)
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

    private IPublishedContent? WalkContentTree(IPublishedContent node, string[] segments, string? culture)
    {
        string segment = segments.First();

        if (segments.Length == 1 && string.Equals(node.UrlSegment(_variationContextAccessor, culture), segment))
        {
            return node;
        }

        string[] childSegments = segments.Skip(1).ToArray();

        foreach (var child in node.Children.EmptyNull())
        {
            if (string.Equals(child.UrlSegment(_variationContextAccessor, culture), segment))
            {
                if (segments.Length == 1)
                    return child;

                var grandChild = WalkContentTree(child, childSegments, culture);

                if (grandChild is not null)
                    return grandChild;
            }

            if (child.IsInvisibleNode(_rulesManager))
            {
                var hiddenChild = WalkContentTree(child, segments, culture);

                if (hiddenChild is not null)
                    return hiddenChild;
            }
        }

        return null;
    }
}