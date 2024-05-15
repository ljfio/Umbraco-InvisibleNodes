// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Notifications;

public class InvalidateCacheNotificationHandler :
    INotificationHandler<ContentSavingNotification>,
    INotificationHandler<ContentUnpublishingNotification>,
    INotificationHandler<ContentMovingNotification>,
    INotificationHandler<ContentMovingToRecycleBinNotification>
{
    private readonly IInvisibleNodeCache _invisibleNodeCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public InvalidateCacheNotificationHandler(
        IInvisibleNodeCache invisibleNodeCache,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _invisibleNodeCache = invisibleNodeCache;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public void Handle(ContentSavingNotification notification)
    {
        foreach (var publishedEntity in notification.SavedEntities.EmptyNull())
            RemoveEntityFromCache(publishedEntity);
    }
    
    public void Handle(ContentUnpublishingNotification notification)
    {
        foreach (var publishedEntity in notification.UnpublishedEntities.EmptyNull())
            RemoveEntityFromCache(publishedEntity);
    }

    public void Handle(ContentMovingNotification notification)
    {
        foreach (var moveEventInfo in notification.MoveInfoCollection.EmptyNull())
            RemoveEntityFromCache(moveEventInfo.Entity);
    }

    public void Handle(ContentMovingToRecycleBinNotification notification)
    {
        foreach (var moveEventInfo in notification.MoveInfoCollection.EmptyNull())
            RemoveEntityFromCache(moveEventInfo.Entity);
    }

    private void RemoveEntityFromCache(IContent entity)
    {
        var cultures = entity.PublishedCultures
            .Append(null)
            .Distinct();

        var uris = cultures
            .Select(culture => _publishedUrlProvider.GetUrl(entity.Id, UrlMode.Absolute, culture))
            .Where(url => !string.IsNullOrEmpty(url) && !url.Equals("#"))
            .Select(url => new Uri(url))
            .Distinct();

        var otherUris = _publishedUrlProvider.GetOtherUrls(entity.Id)
            .Where(info => info.IsUrl)
            .Select(info => new Uri(info.Text));
        
        foreach (var uri in uris.Concat(otherUris)) 
            _invisibleNodeCache.ClearRoute(uri.GetLeftPart(UriPartial.Authority), uri.AbsolutePath);
    }
}