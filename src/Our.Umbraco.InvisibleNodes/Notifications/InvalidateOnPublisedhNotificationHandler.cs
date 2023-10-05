// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Notifications;

public class InvalidateOnPublisedhNotificationHandler : INotificationHandler<ContentPublishedNotification>
{
    private readonly IInvisibleNodeCache _invisibleNodeCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public InvalidateOnPublisedhNotificationHandler(
        IInvisibleNodeCache invisibleNodeCache,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _invisibleNodeCache = invisibleNodeCache;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public void Handle(ContentPublishedNotification notification)
    {
        foreach (var publishedEntity in notification.PublishedEntities.EmptyNull())
        {
            foreach (var culture in publishedEntity.PublishedCultures)
            {
                string url = _publishedUrlProvider.GetUrl(publishedEntity.Id, UrlMode.Absolute, culture);

                var uri = new Uri(url);

                _invisibleNodeCache.ClearRoute(uri.Host, uri.AbsolutePath);
            }
        }
    }
}