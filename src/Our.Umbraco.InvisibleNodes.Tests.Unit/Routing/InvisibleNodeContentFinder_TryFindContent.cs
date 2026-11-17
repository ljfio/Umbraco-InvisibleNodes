// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Our.Umbraco.InvisibleNodes.Routing;
using Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Routing;

public class InvisibleNodeContentFinder_TryFindContent
{
    private const string Host = "https://example.org";

    private readonly FakePublishedContentCache _contentCache;
    private readonly FakeUmbracoContext _umbracoContext;
    private readonly Mock<IInvisibleNodeCache> _cache;
    private readonly Mock<IInvisibleNodeLocator> _locator;

    private readonly IPublishedContent _root;
    private readonly IPublishedContent _section;
    private readonly IPublishedContent _page;

    public InvisibleNodeContentFinder_TryFindContent()
    {
        _contentCache = new FakePublishedContentCache();
        _umbracoContext = new FakeUmbracoContext(
            _contentCache,
            new FakePublishedMediaCache(),
            new FakeDomainCache(string.Empty));

        _cache = new Mock<IInvisibleNodeCache>();
        _locator = new Mock<IInvisibleNodeLocator>();

        _root = _contentCache.Generate("Home", "home");
        _section = _contentCache.Generate("Section", "section", _root);
        _page = _contentCache.Generate("Page", "page", _section);
    }

    [Fact]
    public async Task Should_Find_Content_From_Exact_Cache_Path()
    {
        // Arrange
        _cache.Setup(m => m.GetRoute(Host, "/section/page")).Returns(_page.Id);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
        _cache.Verify(m => m.GetRoute(Host, "/section"), Times.Never);
        _cache.Verify(m => m.StoreRoute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _locator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Find_Content_Below_Cached_Ancestor_Path()
    {
        // Arrange
        _cache.Setup(m => m.GetRoute(Host, "/section")).Returns(_section.Id);

        _locator.Setup(m => m.Locate(_contentCache, _section, "page", string.Empty)).ReturnsAsync(_page);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
        _cache.Verify(m => m.StoreRoute(Host, "/section/page", _page.Id), Times.Once);
        _locator.Verify(m => m.Locate(_contentCache, _section, "page", string.Empty), Times.Once);
        _locator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Use_Nearest_Cached_Ancestor_Path()
    {
        // Arrange
        var nested = _contentCache.Generate("Nested", "nested", _page);

        _cache.Setup(m => m.GetRoute(Host, "/section")).Returns(_section.Id);
        _cache.Setup(m => m.GetRoute(Host, "/section/page")).Returns(_page.Id);

        _locator.Setup(m => m.Locate(_contentCache, _page, "nested", string.Empty)).ReturnsAsync(nested);

        var request = CreateRequest($"{Host}/section/page/nested/");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(nested), Times.Once);
        _cache.Verify(m => m.GetRoute(Host, "/section"), Times.Never);
        _cache.Verify(m => m.StoreRoute(Host, "/section/page/nested/", nested.Id), Times.Once);
        _locator.Verify(m => m.Locate(_contentCache, _page, "nested", string.Empty), Times.Once);
        _locator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Walk_From_Root_When_Not_Below_Cached_Ancestor()
    {
        // Arrange
        _cache.Setup(m => m.GetRoute(Host, "/section")).Returns(_section.Id);

        _locator.Setup(m => m.Locate(_contentCache, _section, "page", string.Empty))
            .ReturnsAsync((IPublishedContent?)null);
        _locator.Setup(m => m.Locate(_contentCache, _root, "section/page", string.Empty)).ReturnsAsync(_page);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
        _cache.Verify(m => m.StoreRoute(Host, "/section/page", _page.Id), Times.Once);
        _cache.Verify(m => m.ClearRoute(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Should_Clear_Stale_Cached_Ancestor_Path()
    {
        // Arrange
        _cache.Setup(m => m.GetRoute(Host, "/section")).Returns(int.MaxValue);

        _locator.Setup(m => m.Locate(_contentCache, _root, "section/page", string.Empty)).ReturnsAsync(_page);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        _cache.Verify(m => m.ClearRoute(Host, "/section"), Times.Once);
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
        _locator.Verify(m => m.Locate(_contentCache, _root, "section/page", string.Empty), Times.Once);
        _locator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Not_Find_Content_When_Not_Cached_Or_Located()
    {
        // Arrange
        var request = CreateRequest($"{Host}/section/missing");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeFalse();
        request.Verify(m => m.SetPublishedContent(It.IsAny<IPublishedContent>()), Times.Never);
        _cache.Verify(m => m.StoreRoute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Should_Walk_From_Domain_Root_With_Path_Relative_To_Domain()
    {
        // Arrange
        _locator.Setup(m => m.Locate(_contentCache, _section, "page", string.Empty)).ReturnsAsync(_page);

        var request = CreateRequest($"{Host}/en/page");
        request.SetupGet(m => m.Domain).Returns(CreateDomain(_section, $"{Host}/en", request.Object.Uri));

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
        _cache.Verify(m => m.StoreRoute(Host, "/en/page", _page.Id), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Use_Cached_Path_Above_Domain_Path()
    {
        // Arrange
        var other = _contentCache.Generate("En", "page", _root);

        // A route cached for another hostname on the same host, e.g. example.org/ assigned to the home node
        _cache.Setup(m => m.GetRoute(Host, "/en")).Returns(other.Id);

        _locator.Setup(m => m.Locate(_contentCache, _section, "page", string.Empty)).ReturnsAsync(_page);

        var request = CreateRequest($"{Host}/en/shop/page");
        request.SetupGet(m => m.Domain).Returns(CreateDomain(_section, $"{Host}/en/shop", request.Object.Uri));

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
        _cache.Verify(m => m.GetRoute(Host, "/en/shop/page"), Times.Once);
        _cache.Verify(m => m.GetRoute(Host, "/en/shop"), Times.Never);
        _cache.Verify(m => m.GetRoute(Host, "/en"), Times.Never);
        _locator.Verify(m => m.Locate(_contentCache, _section, "page", string.Empty), Times.Once);
        _locator.VerifyNoOtherCalls();
    }

    private static DomainAndUri CreateDomain(IPublishedContent content, string url, Uri current) =>
        new(new Domain(1, url, content.Id, null, false, 0), current);

    private InvisibleNodeContentFinder CreateFinder() =>
        new(
            _umbracoContext.GetUmbracoContextAccessor(),
            _cache.Object,
            _locator.Object,
            _contentCache.DocumentNavigationQueryService);

    private static Mock<IPublishedRequestBuilder> CreateRequest(string url)
    {
        var request = new Mock<IPublishedRequestBuilder>();
        request.SetupGet(m => m.Uri).Returns(new Uri(url));
        request.SetupGet(m => m.Culture).Returns((string?)null);
        request.SetupGet(m => m.Domain).Returns((DomainAndUri?)null);
        return request;
    }
}
