// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Our.Umbraco.InvisibleNodes.Notifications;

/// <summary>
/// Clears the cached routes when the content or hostnames change
/// </summary>
/// <remarks>
/// The cache refresher notifications are published on every server, after Umbraco has updated its own caches, for
/// any content change (save, publish, unpublish, move, sort, trash, delete) and any hostname change. A change can
/// alter the route of any number of other nodes (descendants, nodes below an invisible node, nodes below another
/// hostname), and the previous routes are no longer known by this point, so all the cached routes are cleared.
/// </remarks>
public class InvalidateCacheNotificationHandler :
    INotificationHandler<ContentCacheRefresherNotification>,
    INotificationHandler<DomainCacheRefresherNotification>
{
    private readonly IInvisibleNodeCache _invisibleNodeCache;

    public InvalidateCacheNotificationHandler(IInvisibleNodeCache invisibleNodeCache)
    {
        _invisibleNodeCache = invisibleNodeCache;
    }

    public void Handle(ContentCacheRefresherNotification notification)
    {
        // Blueprints are not routable
        if (notification.MessageObject is ContentCacheRefresher.JsonPayload[] payloads &&
            payloads.Length > 0 &&
            payloads.All(payload => payload.Blueprint))
            return;

        _invisibleNodeCache.ClearAll();
    }

    public void Handle(DomainCacheRefresherNotification notification)
    {
        _invisibleNodeCache.ClearAll();
    }
}
