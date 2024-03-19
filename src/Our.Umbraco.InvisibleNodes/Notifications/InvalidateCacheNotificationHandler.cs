// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Notifications;

public class InvalidateCacheNotificationHandler :
    INotificationHandler<ContentSavingNotification>,
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
        foreach (var culture in entity.PublishedCultures)
        {
            string url = _publishedUrlProvider.GetUrl(entity.Id, UrlMode.Absolute, culture);
            
            if (string.IsNullOrEmpty(url) || url.Equals("#"))
                continue;

            var uri = new Uri(url);

            _invisibleNodeCache.ClearRoute(uri.Host, uri.AbsolutePath);
        }
    }
}