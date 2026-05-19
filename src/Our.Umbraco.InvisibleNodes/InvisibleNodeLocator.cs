// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes;

public class InvisibleNodeLocator : IInvisibleNodeLocator
{
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly IInvisibleNodeRulesManager _rulesManager;
    private readonly IDocumentUrlService _documentUrlService;

    public InvisibleNodeLocator(
        IDocumentNavigationQueryService navigationQueryService,
        IDocumentUrlService documentUrlService,
        IInvisibleNodeRulesManager rulesManager)
    {
        _navigationQueryService = navigationQueryService;
        _documentUrlService = documentUrlService;
        _rulesManager = rulesManager;
    }

    /// <inheritdoc />
    public async Task<IPublishedContent?> Locate(
        IPublishedContentCache cache,
        IPublishedContent node,
        string path,
        string culture)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        string trimmedPath = path.Trim('/');

        if (string.IsNullOrEmpty(trimmedPath))
            return null;

        string[] segments = trimmedPath.Split('/');

        if (segments.Length == 0)
            return null;

        return await WalkContentTree(cache, node, segments, culture);
    }

    private async Task<IPublishedContent?> WalkContentTree(
        IPublishedContentCache cache,
        IPublishedContent node,
        string[] segments,
        string culture)
    {
        string segment = segments.First();

        if (!_navigationQueryService.TryGetChildrenKeys(node.Key, out var keys))
            return null;

        foreach (var key in keys)
        {
            var child = await cache.GetByIdAsync(key);

            if (child is null)
                continue;

            var childSegment = _documentUrlService.GetUrlSegment(child.Key, culture, child.IsDraft(culture));

            if (string.Equals(childSegment, segment))
            {
                if (segments.Length == 1)
                    return child;

                string[] childSegments = segments.Skip(1).ToArray();

                var grandChild = await WalkContentTree(cache, child, childSegments, culture);

                if (grandChild is not null)
                    return grandChild;
            }

            if (child.IsInvisibleNode(_rulesManager))
            {
                var hiddenChild = await WalkContentTree(cache, child, segments, culture);

                if (hiddenChild is not null)
                    return hiddenChild;
            }
        }

        return null;
    }
}