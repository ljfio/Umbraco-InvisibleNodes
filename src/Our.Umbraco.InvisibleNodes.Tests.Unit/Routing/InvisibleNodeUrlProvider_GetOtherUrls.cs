// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Routing;
using Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
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

    private static IRuntimeState RuntimeState(RuntimeLevel level) =>
        Mock.Of<IRuntimeState>(r => r.Level == level);

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
            RequestHandlerOptions,
            RuntimeState(RuntimeLevel.Run));

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
            RequestHandlerOptions,
            RuntimeState(RuntimeLevel.Run));

        // Act
        var urls = provider.GetOtherUrls(root.Id, uri).ToArray();

        // Assert
        urls.Should()
            .NotBeNullOrEmpty()
            .And
            .ContainSingle(value => Equals(otherUri, value.Url));
    }

    [Fact]
    public void Should_Return_EmptyForInvisibleNode_WithMultipleHostnames()
    {
        // Arrange
        var root = _contentCache.Generate("Home", "home");
        var news = _contentCache.Generate("News", "page", root);
        var invisible = _contentCache.Generate("2026", "folder", news);
        _contentCache.Generate("Article", "page", invisible);

        _domainCache.AddRange(root.Id, "example.org", "example.com");

        var provider = CreateProvider(InvisibleRulesManager(invisible));

        // Act
        var urls = provider.GetOtherUrls(invisible.Id, new Uri("https://example.org/"));

        // Assert
        urls.Should().BeEmpty();
    }

    [Fact]
    public void Should_Return_1UrlForHiddenDescendant_WithMultipleHostnames()
    {
        // Arrange
        var root = _contentCache.Generate("Home", "home");
        var news = _contentCache.Generate("News", "page", root);
        var invisible = _contentCache.Generate("2026", "folder", news);
        var article = _contentCache.Generate("Article", "page", invisible);

        _domainCache.AddRange(root.Id, "example.org", "example.com");

        var provider = CreateProvider(InvisibleRulesManager(invisible));

        // Act
        var urls = provider.GetOtherUrls(article.Id, new Uri("https://example.org/")).ToArray();

        // Assert
        urls.Should()
            .ContainSingle()
            .Which.Url.Should().Be(new Uri("https://example.com/news/article/"));
    }

    private static IInvisibleNodeRulesManager InvisibleRulesManager(params IPublishedContent[] invisible)
    {
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns((IPublishedContent content) => invisible.Contains(content));

        return mockRulesManager.Object;
    }

    private InvisibleNodeUrlProvider CreateProvider(IInvisibleNodeRulesManager rulesManager) =>
        new(
            _umbracoContext.GetUmbracoContextAccessor(),
            new SiteDomainMapper(),
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager,
            RequestHandlerOptions,
            RuntimeState(RuntimeLevel.Run));
}