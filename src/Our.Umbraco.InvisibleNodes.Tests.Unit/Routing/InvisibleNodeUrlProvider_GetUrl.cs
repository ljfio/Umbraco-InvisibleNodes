using System;
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

public class InvisibleNodeUrlProvider_GetUrl
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

    public InvisibleNodeUrlProvider_GetUrl()
    {
        _contentCache = new FakePublishedContentCache();
        var mediaCache = new FakePublishedMediaCache();
        _domainCache = new FakeDomainCache(string.Empty);

        _filteringService = new FakePublishedStatusFilteringService(_contentCache);

        _umbracoContext = new FakeUmbracoContext(_contentCache, mediaCache, _domainCache);
    }

    #region Default URL Mode

    [Fact]
    public void Should_Return_DefaultRoot()
    {
        // Arrange
        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(root, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("/", UriKind.Relative));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_DefaultNested1Level()
    {
        // Arrange
        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);


        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(page, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("/page/", UriKind.Relative));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_DefaultNested2Levels()
    {
        // Arrange
        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var nested = _contentCache.Generate("Nested", "nested", page);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(nested, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("/page/nested/", UriKind.Relative));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_Child_Same_Name()
    {
        // Arrange
        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var nested = _contentCache.Generate("Page", "page", page);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(nested, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("/page/page/", UriKind.Relative));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_DefaultInvisible()
    {
        // Arrange
        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var invisible = _contentCache.Generate("Invisible", "invisible", page);

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsNotIn(invisible)))
            .Returns(false);

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsIn(invisible)))
            .Returns(true);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(invisible, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("/page/", UriKind.Relative));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_DefaultNestedHidden()
    {
        // Arrange
        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var invisible = _contentCache.Generate("Invisible", "invisible", page);
        var hidden = _contentCache.Generate("Hidden", "hidden", invisible);

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsNotIn(invisible)))
            .Returns(false);

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsIn(invisible)))
            .Returns(true);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(hidden, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("/page/hidden/", UriKind.Relative));
        url.Culture.Should().BeNull();
    }

    #endregion

    #region Absolute URL Mode

    [Fact]
    public void Should_Return_AbsoluteRoot()
    {
        // Arrange
        var domain = _domainCache.Add(1, "example.org");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(root, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("https://example.org/"));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_AbsoluteNested1Level()
    {
        // Arrange
        var domain = _domainCache.Add(1, "example.org");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(page, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("https://example.org/page/"));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_AbsoluteNested2Levels()
    {
        // Arrange
        var domain = _domainCache.Add(1, "example.org");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var nested = _contentCache.Generate("Nested", "nested", page);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(nested, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("https://example.org/page/nested/"));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_AbsoluteInvisible()
    {
        // Arrange
        var domain = _domainCache.Add(1, "example.org");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var invisible = _contentCache.Generate("Invisible", "invisible", page);

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsNotIn(invisible)))
            .Returns(false);

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsIn(invisible)))
            .Returns(true);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(invisible, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("https://example.org/page/"));
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void Should_Return_AbsoluteNestedHidden()
    {
        // Arrange
        var domain = _domainCache.Add(1, "example.org");

        var umbracoContextAccessor = _umbracoContext.GetUmbracoContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = _contentCache.Generate("Home", "home");
        var page = _contentCache.Generate("Page", "page", root);
        var invisible = _contentCache.Generate("Invisible", "invisible", page);
        var hidden = _contentCache.Generate("Hidden", "hidden", invisible);

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsNotIn(invisible)))
            .Returns(false);

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsIn(invisible)))
            .Returns(true);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            siteDomainMapper,
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            _filteringService,
            rulesManager.Object,
            RequestHandlerOptions);

        // Act
        var url = provider.GetUrl(hidden, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Url.Should().Be(new Uri("https://example.org/page/hidden/"));
        url.Culture.Should().BeNull();
    }

    #endregion
}