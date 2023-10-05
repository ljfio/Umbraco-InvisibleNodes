// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.InvisibleNodes.Notifications;

public class InvalidateOnMovedNotificationHandler :
    INotificationHandler<ContentMovingNotification>,
    INotificationHandler<ContentMovedNotification>,
    INotificationHandler<ContentMovingToRecycleBinNotification>,
    INotificationHandler<ContentMovedToRecycleBinNotification>
{
    private readonly IInvisibleNodeCache _invisibleNodeCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    private readonly IDictionary<int, IList<string>> _previousUrls = new Dictionary<int, IList<string>>();

    public InvalidateOnMovedNotificationHandler(
        IInvisibleNodeCache invisibleNodeCache,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _invisibleNodeCache = invisibleNodeCache;
        _publishedUrlProvider = publishedUrlProvider;
    }

    #region Content Tree

    public void Handle(ContentMovingNotification notification)
    {
        foreach (var moveEventInfo in notification.MoveInfoCollection)
            HandleMoving(moveEventInfo.Entity);
    }

    public void Handle(ContentMovedNotification notification)
    {
        foreach (var moveEventInfo in notification.MoveInfoCollection) 
            HandleMoved(moveEventInfo.Entity);
    }
    
    #endregion

    #region Recycle Bin

    public void Handle(ContentMovingToRecycleBinNotification notification)
    {
        foreach (var moveEventInfo in notification.MoveInfoCollection)
            HandleMoving(moveEventInfo.Entity);
    }

    public void Handle(ContentMovedToRecycleBinNotification notification)
    {
        foreach (var moveEventInfo in notification.MoveInfoCollection) 
            HandleMoved(moveEventInfo.Entity);
    }
    
    #endregion

    #region Common / Helper

    private void HandleMoving(IContent entity)
    {
        var urls = new List<string>();

        foreach (var culture in entity.PublishedCultures)
        {
            urls.Add(_publishedUrlProvider.GetUrl(entity.Id, UrlMode.Absolute, culture));
        }

        _previousUrls[entity.Id] = urls;
    }

    private void HandleMoved(IContent entity)
    {
        if (_previousUrls.TryGetValue(entity.Id, out var urls))
        {
            foreach (var url in urls)
            {
                var uri = new Uri(url);
                _invisibleNodeCache.ClearRoute(uri.Host, uri.AbsolutePath);
            }

            _previousUrls.Remove(entity.Id);
        }
    }

    #endregion
}