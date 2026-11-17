// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Our.Umbraco.InvisibleNodes.Caching;
using Our.Umbraco.InvisibleNodes.Composing;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Composing;

public class PackageComposer_Compose
{
    [Theory]
    [InlineData(null, typeof(InvisibleNodeCache))]
    [InlineData("true", typeof(InvisibleNodeCache))]
    [InlineData("false", typeof(NoOpNodeCache))]
    public void Should_Register_Cache_From_Settings(string? cachingEnabled, System.Type expected)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(AppCaches.Disabled);

        // Left out of the configuration when null, so the default is used
        var settings = new Dictionary<string, string?>();

        if (cachingEnabled is not null)
            settings["InvisibleNodes:CachingEnabled"] = cachingEnabled;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var builder = new Mock<IUmbracoBuilder>();
        builder.SetupGet(m => m.Services).Returns(services);
        builder.SetupGet(m => m.Config).Returns(configuration);
        builder.Setup(m => m.WithCollectionBuilder<ContentFinderCollectionBuilder>())
            .Returns(new ContentFinderCollectionBuilder());
        builder.Setup(m => m.WithCollectionBuilder<UrlProviderCollectionBuilder>())
            .Returns(new UrlProviderCollectionBuilder());

        // Act
        new PackageComposer().Compose(builder.Object);

        // Assert
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<IInvisibleNodeCache>();
        cache.Should().BeOfType(expected);
    }

    [Fact]
    public void Should_Not_Store_Routes_When_Caching_Disabled()
    {
        // Arrange
        var cache = new NoOpNodeCache();

        // Act
        cache.StoreRoute("https://example.org", "/page", 1);

        // Assert, so every request walks the content tree
        cache.GetRoute("https://example.org", "/page").Should().BeNull();
    }
}
