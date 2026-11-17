// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Moq;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Our.Umbraco.InvisibleNodes.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Notifications;

public class InvalidateCacheNotificationHandler_Handle
{
    [Theory]
    [InlineData(TreeChangeTypes.RefreshNode)]
    [InlineData(TreeChangeTypes.RefreshBranch)]
    [InlineData(TreeChangeTypes.RefreshAll)]
    [InlineData(TreeChangeTypes.Remove)]
    public void Should_ClearAll_When_Content_Changes(TreeChangeTypes changeTypes)
    {
        // Arrange
        var cache = new Mock<IInvisibleNodeCache>();

        var payloads = new[]
        {
            new ContentCacheRefresher.JsonPayload { Id = 1, ChangeTypes = changeTypes },
        };

        var notification = new ContentCacheRefresherNotification(payloads, MessageType.RefreshByPayload);

        var handler = new InvalidateCacheNotificationHandler(cache.Object);

        // Act
        handler.Handle(notification);

        // Assert
        cache.Verify(m => m.ClearAll(), Times.Once);
        cache.VerifyNoOtherCalls();
    }

    [Fact]
    public void Should_ClearAll_When_Content_Changes_Alongside_Blueprint()
    {
        // Arrange
        var cache = new Mock<IInvisibleNodeCache>();

        var payloads = new[]
        {
            new ContentCacheRefresher.JsonPayload { Id = 1, ChangeTypes = TreeChangeTypes.RefreshNode, Blueprint = true },
            new ContentCacheRefresher.JsonPayload { Id = 2, ChangeTypes = TreeChangeTypes.RefreshNode },
        };

        var notification = new ContentCacheRefresherNotification(payloads, MessageType.RefreshByPayload);

        var handler = new InvalidateCacheNotificationHandler(cache.Object);

        // Act
        handler.Handle(notification);

        // Assert
        cache.Verify(m => m.ClearAll(), Times.Once);
    }

    [Fact]
    public void Should_Not_Clear_When_Only_Blueprints_Change()
    {
        // Arrange
        var cache = new Mock<IInvisibleNodeCache>();

        var payloads = new[]
        {
            new ContentCacheRefresher.JsonPayload { Id = 1, ChangeTypes = TreeChangeTypes.RefreshNode, Blueprint = true },
        };

        var notification = new ContentCacheRefresherNotification(payloads, MessageType.RefreshByPayload);

        var handler = new InvalidateCacheNotificationHandler(cache.Object);

        // Act
        handler.Handle(notification);

        // Assert
        cache.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(DomainChangeTypes.Refresh)]
    [InlineData(DomainChangeTypes.Remove)]
    [InlineData(DomainChangeTypes.RefreshAll)]
    public void Should_ClearAll_When_Domains_Change(DomainChangeTypes changeType)
    {
        // Arrange
        var cache = new Mock<IInvisibleNodeCache>();

        var payloads = new[]
        {
            new DomainCacheRefresher.JsonPayload(1, changeType),
        };

        var notification = new DomainCacheRefresherNotification(payloads, MessageType.RefreshByPayload);

        var handler = new InvalidateCacheNotificationHandler(cache.Object);

        // Act
        handler.Handle(notification);

        // Assert
        cache.Verify(m => m.ClearAll(), Times.Once);
        cache.VerifyNoOtherCalls();
    }
}
