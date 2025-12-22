// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Routing;
using Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Routing;

public class InvisibleNodeUrlProvider_GetOtherUrls
{
    private readonly FakePublishedContentCache _contentCache;
    private readonly FakeDomainCache _domainCache;
    private readonly FakePublishedStatusFilteringService _filteringService;
    private readonly FakeUmbracoContext _umbracoContext;

    private static readonly IOptions<RequestHandlerSettings> RequestHandlerOptions = Options.Create(
        new RequestHandlerSettings
        {
            AddTrailingSlash = true,
        });

    public InvisibleNodeUrlProvider_GetOtherUrls()
    {
        _contentCache = new FakePublishedContentCache();
        var mediaCache = new FakePublishedMediaCache();
        _domainCache = new FakeDomainCache(string.Empty);

        _filteringService = new FakePublishedStatusFilteringService(_contentCache);

        _umbracoContext = new FakeUmbracoContext(_contentCache, mediaCache, _domainCache);
    }

    [Fact]
    public void Should_Return_EmptyForMatchingRoot()
    {
        // Arrange
        var root = _contentCache.Generate("Home", "home");
        var domain = _domainCache.Add(root.Id, "example.org");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            mockRulesManager.Object,
            RequestHandlerOptions);

        // Act
        var urls = provider.GetOtherUrls(root.Id, uri);

        // Assert
        urls.Should().NotBeNull();
        urls.Should().BeEmpty();
    }

    [Fact]
    public void Should_Return_1UrlForMatchingRoot()
    {
        // Arrange
        var root = _contentCache.Generate("Home", "home");
        var domains = _domainCache.AddRange(root.Id, "example.org", "example.com");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var uri = new Uri("https://example.org/");
        var otherUri = new Uri("https://example.com/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            mockRulesManager.Object,
            RequestHandlerOptions);

        // Act
        var urls = provider.GetOtherUrls(root.Id, uri).ToArray();

        // Assert
        urls.Should()
            .NotBeNullOrEmpty()
            .And
            .ContainSingle(value => Equals(otherUri, value.Url));
    }
}