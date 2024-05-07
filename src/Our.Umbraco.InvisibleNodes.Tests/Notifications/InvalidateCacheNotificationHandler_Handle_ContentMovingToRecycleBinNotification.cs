// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Our.Umbraco.InvisibleNodes.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.InvisibleNodes.Tests.Notifications;

public class InvalidateCacheNotificationHandler_Handle_ContentMovingToRecycleBinNotification
{
    [Fact]
    public void Should_Call_InvisibleNodeCache_ClearRoute()
    {
        // Arrange
        int id = 1;
        
        var content = new Mock<IContent>();
        content.SetupGet(m => m.Id)
            .Returns(id);
        content.SetupGet(m => m.PublishedCultures)
            .Returns(new[] { "en-US" });
        
        var cache = new Mock<IInvisibleNodeCache>();
        
        var provider = new Mock<IPublishedUrlProvider>();
        provider.Setup(m => m.GetUrl(id, UrlMode.Absolute, "en-US", null))
            .Returns("https://example.org/home/");
            
        var moveEvent = new MoveEventInfo<IContent>(content.Object, "/home/", 2);
        var messages = new EventMessages();
        
        var notification = new ContentMovingToRecycleBinNotification(moveEvent, messages);
        
        var handler = new InvalidateCacheNotificationHandler(cache.Object, provider.Object);
        
        // Act
        handler.Handle(notification);
        
        // Assert
        provider.Verify(m => m.GetUrl(id, UrlMode.Absolute, "en-US", null), Times.Once);
        provider.Verify(m => m.GetUrl(id, UrlMode.Absolute, null, null), Times.Once);
        provider.Verify(m => m.GetOtherUrls(id), Times.Once);
        provider.VerifyNoOtherCalls();
        
        cache.Verify(m => m.ClearRoute("example.org", "/home/"), Times.Once);
        cache.VerifyNoOtherCalls();
    }
}