// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Our.Umbraco.InvisibleNodes.Routing;
using Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Routing;

public class InvisibleNodeContentFinder_TryFindContent
{
    private const string Host = "https://example.org";

    private readonly FakePublishedContentCache _contentCache;
    private readonly FakeUmbracoContext _umbracoContext;
    private readonly Mock<IInvisibleNodeCache> _cache;
    private readonly Mock<IInvisibleNodeLocator> _locator;
    private readonly Mock<IDocumentUrlService> _documentUrlService;
    private readonly Mock<IPublishedUrlProvider> _publishedUrlProvider;
    private readonly Mock<IInvisibleNodeRulesManager> _rulesManager;
    private readonly InvisibleNodeSettings _settings;

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
        _documentUrlService = new Mock<IDocumentUrlService>();
        _publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        _rulesManager = new Mock<IInvisibleNodeRulesManager>();
        _settings = new InvisibleNodeSettings();

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

    #region Native URLs

    [Fact]
    public async Task Should_Redirect_Native_Url_Below_Invisible_Node()
    {
        // Arrange
        _rulesManager.Setup(m => m.IsInvisibleNode(_section)).Returns(true);

        var request = CreateRequest($"{Host}/section/page?page=2");

        _documentUrlService.Setup(m => m.GetDocumentKeyByRoute("/section/page", null, null, false))
            .Returns(_page.Key);
        _publishedUrlProvider.Setup(m => m.GetUrl(_page, UrlMode.Default, null, request.Object.Uri))
            .Returns("/page/");

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetRedirectPermanent("/page/?page=2"), Times.Once);
        request.Verify(m => m.SetPublishedContent(It.IsAny<IPublishedContent>()), Times.Never);
        _cache.Verify(m => m.StoreRoute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Should_Redirect_Native_Url_Relative_To_Domain()
    {
        // Arrange
        var invisible = _contentCache.Generate("Invisible", "invisible", _section);
        var nested = _contentCache.Generate("Nested", "page", invisible);

        _rulesManager.Setup(m => m.IsInvisibleNode(invisible)).Returns(true);

        var request = CreateRequest($"{Host}/en/invisible/nested");
        request.SetupGet(m => m.Domain).Returns(CreateDomain(_section, $"{Host}/en", request.Object.Uri));

        _documentUrlService.Setup(m => m.GetDocumentKeyByRoute("/invisible/nested", null, _section.Id, false))
            .Returns(nested.Key);
        _publishedUrlProvider.Setup(m => m.GetUrl(nested, UrlMode.Default, null, request.Object.Uri))
            .Returns("/en/nested/");

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetRedirectPermanent("/en/nested/"), Times.Once);
    }

    [Theory]
    [InlineData(NativeUrlBehavior.Redirect)]
    [InlineData(NativeUrlBehavior.NotFound)]
    public async Task Should_Not_Find_Content_For_Native_Url_Of_Invisible_Node(NativeUrlBehavior behavior)
    {
        // Arrange
        _settings.NativeUrls = behavior;
        _rulesManager.Setup(m => m.IsInvisibleNode(_section)).Returns(true);

        _documentUrlService.Setup(m => m.GetDocumentKeyByRoute("/section", null, null, false))
            .Returns(_section.Key);

        var request = CreateRequest($"{Host}/section");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert, handled without content so no other content finder serves it
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(It.IsAny<IPublishedContent>()), Times.Never);
        request.Verify(m => m.SetRedirectPermanent(It.IsAny<string>()), Times.Never);
        _publishedUrlProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Not_Redirect_Native_Url_When_NotFound()
    {
        // Arrange
        _settings.NativeUrls = NativeUrlBehavior.NotFound;
        _rulesManager.Setup(m => m.IsInvisibleNode(_section)).Returns(true);

        _documentUrlService.Setup(m => m.GetDocumentKeyByRoute("/section/page", null, null, false))
            .Returns(_page.Key);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(It.IsAny<IPublishedContent>()), Times.Never);
        request.Verify(m => m.SetRedirectPermanent(It.IsAny<string>()), Times.Never);
        _publishedUrlProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Leave_Native_Url_When_Allowed()
    {
        // Arrange
        _settings.NativeUrls = NativeUrlBehavior.Allow;
        _rulesManager.Setup(m => m.IsInvisibleNode(_section)).Returns(true);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert, left for Umbraco to serve
        found.Should().BeFalse();
        request.Verify(m => m.SetRedirectPermanent(It.IsAny<string>()), Times.Never);
        _documentUrlService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Should_Leave_Url_Without_Invisible_Node()
    {
        // Arrange
        _documentUrlService.Setup(m => m.GetDocumentKeyByRoute("/section/page", null, null, false))
            .Returns(_page.Key);

        var request = CreateRequest($"{Host}/section/page");
        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeFalse();
        request.Verify(m => m.SetRedirectPermanent(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("#")]
    [InlineData("")]
    public async Task Should_Not_Redirect_Native_Url_When_Target_Has_No_Url(string url)
    {
        // Arrange
        _rulesManager.Setup(m => m.IsInvisibleNode(_section)).Returns(true);

        var request = CreateRequest($"{Host}/section/page");

        _documentUrlService.Setup(m => m.GetDocumentKeyByRoute("/section/page", null, null, false))
            .Returns(_page.Key);
        _publishedUrlProvider.Setup(m => m.GetUrl(_page, UrlMode.Default, null, request.Object.Uri))
            .Returns(url);

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert, handled without content so the request is not found rather than redirected
        found.Should().BeTrue();
        request.Verify(m => m.SetRedirectPermanent(It.IsAny<string>()), Times.Never);
        request.Verify(m => m.SetPublishedContent(It.IsAny<IPublishedContent>()), Times.Never);
    }

    #endregion

    #region Root

    [Fact]
    public async Task Should_Walk_From_Root_With_Request_Culture()
    {
        // Arrange
        var danishRoot = _contentCache.Generate("Hjem", "home", culture: "da-DK");
        var danishPage = _contentCache.Generate("Side", "page", danishRoot, culture: "da-DK");

        _locator.Setup(m => m.Locate(_contentCache, danishRoot, "side", "da-DK")).ReturnsAsync(danishPage);

        var request = CreateRequest($"{Host}/side");
        request.SetupGet(m => m.Culture).Returns("da-DK");

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(danishPage), Times.Once);
        _locator.Verify(m => m.Locate(_contentCache, _root, It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Should_Fall_Back_To_First_Root_Without_Request_Culture()
    {
        // Arrange
        _contentCache.Generate("Home GB", "home", culture: "en-GB");

        _locator.Setup(m => m.Locate(_contentCache, _root, "section/page", "da-DK")).ReturnsAsync(_page);

        var request = CreateRequest($"{Host}/section/page");
        request.SetupGet(m => m.Culture).Returns("da-DK");

        var finder = CreateFinder();

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeTrue();
        request.Verify(m => m.SetPublishedContent(_page), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Find_Content_Without_Root()
    {
        // Arrange
        var emptyCache = new FakePublishedContentCache();
        var emptyContext = new FakeUmbracoContext(
            emptyCache,
            new FakePublishedMediaCache(),
            new FakeDomainCache(string.Empty));

        var finder = new InvisibleNodeContentFinder(
            emptyContext.GetUmbracoContextAccessor(),
            _cache.Object,
            _locator.Object,
            emptyCache.DocumentNavigationQueryService,
            _documentUrlService.Object,
            _publishedUrlProvider.Object,
            _rulesManager.Object,
            Mock.Of<IOptionsMonitor<InvisibleNodeSettings>>(m => m.CurrentValue == _settings));

        var request = CreateRequest($"{Host}/section/page");

        // Act
        bool found = await finder.TryFindContent(request.Object);

        // Assert
        found.Should().BeFalse();
        _locator.VerifyNoOtherCalls();
        request.Verify(m => m.SetPublishedContent(It.IsAny<IPublishedContent>()), Times.Never);
    }

    #endregion

    private static DomainAndUri CreateDomain(IPublishedContent content, string url, Uri current) =>
        new(new Domain(1, url, content.Id, null, false, 0), current);

    private InvisibleNodeContentFinder CreateFinder() =>
        new(
            _umbracoContext.GetUmbracoContextAccessor(),
            _cache.Object,
            _locator.Object,
            _contentCache.DocumentNavigationQueryService,
            _documentUrlService.Object,
            _publishedUrlProvider.Object,
            _rulesManager.Object,
            Mock.Of<IOptionsMonitor<InvisibleNodeSettings>>(m => m.CurrentValue == _settings));

    private static Mock<IPublishedRequestBuilder> CreateRequest(string url)
    {
        var request = new Mock<IPublishedRequestBuilder>();
        request.SetupGet(m => m.Uri).Returns(new Uri(url));
        request.SetupGet(m => m.Culture).Returns((string?)null);
        request.SetupGet(m => m.Domain).Returns((DomainAndUri?)null);
        return request;
    }
}
