// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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

        return await WalkContentTree(cache, node.Key, segments, 0, culture);
    }

    private async Task<IPublishedContent?> WalkContentTree(
        IPublishedContentCache cache,
        Guid parentKey,
        string[] segments,
        int index,
        string culture)
    {
        string segment = segments[index];

        if (!_navigationQueryService.TryGetChildrenKeys(parentKey, out var keys))
            return null;

        // When the rules only depend on the content type the invisible children are known up front, so the visible
        // children whose segment can't match are skipped without loading their content
        var invisibleKeys = GetInvisibleChildrenKeys(parentKey);

        foreach (var key in keys)
        {
            bool? isInvisible = invisibleKeys?.Contains(key);

            if (isInvisible is false && !SegmentMayMatch(key, segment, culture))
                continue;

            var child = await cache.GetByIdAsync(key);

            if (child is null)
                continue;

            // Invisible nodes do not have a segment in the path, so only their descendants can match
            if (isInvisible ?? child.IsInvisibleNode(_rulesManager))
            {
                var hiddenChild = await WalkContentTree(cache, child.Key, segments, index, culture);

                if (hiddenChild is not null)
                    return hiddenChild;

                continue;
            }

            var childSegment = _documentUrlService.GetUrlSegment(child.Key, culture, child.IsDraft(culture));

            if (!string.Equals(childSegment, segment))
                continue;

            if (index == segments.Length - 1)
                return child;

            var grandChild = await WalkContentTree(cache, child.Key, segments, index + 1, culture);

            if (grandChild is not null)
                return grandChild;
        }

        return null;
    }

    /// <summary>
    /// Gets the keys of the invisible children, or <c>null</c> when the rules don't depend only on the content type
    /// </summary>
    private HashSet<Guid>? GetInvisibleChildrenKeys(Guid parentKey)
    {
        var contentTypes = _rulesManager.InvisibleContentTypes;

        if (contentTypes is null)
            return null;

        var invisibleKeys = new HashSet<Guid>();

        foreach (string contentType in contentTypes)
            if (_navigationQueryService.TryGetChildrenKeysOfType(parentKey, contentType, out var keys))
                invisibleKeys.UnionWith(keys);

        return invisibleKeys;
    }

    /// <summary>
    /// Checks the published and draft segments, as whether the child is a draft isn't known until it is loaded
    /// </summary>
    private bool SegmentMayMatch(Guid key, string segment, string culture)
    {
        return string.Equals(_documentUrlService.GetUrlSegment(key, culture, false), segment) ||
               string.Equals(_documentUrlService.GetUrlSegment(key, culture, true), segment);
    }
}