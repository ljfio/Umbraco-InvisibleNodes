using FluentAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests;

public class InvisibleNodeUrlProviderTests
{
    #region Default URL Mode

    [Fact]
    public void ReturnsDefaultRoot()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = GenerateNode("Home", "home");
        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(root, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsDefaultNested1Level()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(page, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/page/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsDefaultNested2Levels()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var nested = GenerateNode("Nested", "nested", page);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(nested, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/page/nested/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsDefaultInvisible()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var invisible = GenerateNode("Invisible", "invisible", page);

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
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(invisible, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/page/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsDefaultNestedHidden()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var invisible = GenerateNode("Invisible", "invisible", page);
        var hidden = GenerateNode("Hidden", "hidden", invisible);

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
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(hidden, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/page/hidden/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    #endregion

    #region Absolute URL Mode

    [Fact]
    public void ReturnsAbsoluteRoot()
    {
        // Arrange
        var domain = GenerateDomain("example.org", 1);
        var umbracoContextAccessor = GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = GenerateNode("Home", "home");
        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(root, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("https://example.org/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsAbsoluteNested1Level()
    {
        // Arrange
        var domain = GenerateDomain("example.org", 1);
        var umbracoContextAccessor = GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(page, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("https://example.org/page/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsAbsoluteNested2Levels()
    {
        // Arrange
        var domain = GenerateDomain("example.org", 1);
        var umbracoContextAccessor = GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var nested = GenerateNode("Nested", "nested", page);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(nested, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("https://example.org/page/nested/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsAbsoluteInvisible()
    {
        // Arrange
        var domain = GenerateDomain("example.org", 1);
        var umbracoContextAccessor = GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var invisible = GenerateNode("Invisible", "invisible", page);

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
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(invisible, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("https://example.org/page/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void ReturnsAbsoluteNestedHidden()
    {
        // Arrange
        var domain = GenerateDomain("example.org", 1);
        var umbracoContextAccessor = GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var invisible = GenerateNode("Invisible", "invisible", page);
        var hidden = GenerateNode("Hidden", "hidden", invisible);

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
            variationContextAccessor,
            siteDomainMapper,
            rulesManager.Object);

        // Act
        var url = provider.GetUrl(hidden, UrlMode.Absolute, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("https://example.org/page/hidden/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    #endregion

    private IEnumerable<Domain> GenerateDomains(params string[] urls)
    {
        int current = 1;

        foreach (var url in urls)
        {
            yield return GenerateDomain(url, current++);
        }
    }

    private Domain GenerateDomain(string url, int id)
    {
#if NET7_0_OR_GREATER
        return new Domain(id, url, id, null, false, id);
#else
        return new Domain(id, url, id, null, false);
#endif
    }

    private IUmbracoContextAccessor GenerateUmbracoContextAccessor(
        IEnumerable<Domain>? domains = null,
        IEnumerable<IPublishedContent>? content = null)
    {
        var mockUmbracoContextAccessor = new Mock<IUmbracoContextAccessor>();

        var umbracoContext = GenerateUmbracoContext(domains, content);

        mockUmbracoContextAccessor
            .Setup(m => m.TryGetUmbracoContext(out umbracoContext))
            .Returns(true);

        return mockUmbracoContextAccessor.Object;
    }

    private IUmbracoContext GenerateUmbracoContext(
        IEnumerable<Domain>? domains = null,
        IEnumerable<IPublishedContent>? content = null)
    {
        var mockDomainCache = new Mock<IDomainCache>();

        if (domains is not null)
        {
            mockDomainCache
                .Setup(m => m.GetAll(It.IsAny<bool>()))
                .Returns(domains);
        }

        var mockPublishedContentCache = new Mock<IPublishedContentCache>();

        if (content is not null)
        {
            mockPublishedContentCache
                .Setup(m => m.GetById(It.IsAny<int>()))
                .Returns((int id) => content.FirstOrDefault(c => c.Id == id));
        }

        var mockUmbracoContext = new Mock<IUmbracoContext>();

        mockUmbracoContext
            .Setup(m => m.Domains)
            .Returns(mockDomainCache.Object);

        mockUmbracoContext
            .Setup(m => m.Content)
            .Returns(mockPublishedContentCache.Object);

        return mockUmbracoContext.Object;
    }

    private IPublishedContent GenerateNode(
        string name,
        string segment,
        IPublishedContent? parent = null,
        string? culture = null)
    {
        var mock = new Mock<IPublishedContent>();
        var mockType = new Mock<IPublishedContentType>();

        mockType.Setup(m => m.Variations)
            .Returns(string.IsNullOrEmpty(culture) ? ContentVariation.Nothing : ContentVariation.Culture);

        mock.Setup(m => m.ContentType)
            .Returns(mockType.Object);

        string key = culture ?? string.Empty;

        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [key] = new(key, name, segment, DateTime.Now)
        };

        mock.Setup(m => m.Level)
            .Returns(parent?.Level + 1 ?? 1);

        mock.Setup(m => m.Cultures)
            .Returns(cultures);

        mock.Setup(m => m.Parent)
            .Returns(parent);

        return mock.Object;
    }
}