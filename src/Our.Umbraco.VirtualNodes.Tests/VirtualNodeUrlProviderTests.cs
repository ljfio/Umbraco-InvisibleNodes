using FluentAssertions;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.VirtualNodes.Tests;

public class VirtualNodeUrlProviderTests
{
    [Fact]
    public void DefaultRoot()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var uri = new Uri("https://example.org/");

        var provider = new VirtualNodeUrlProvider(umbracoContextAccessor, variationContextAccessor, siteDomainMapper);

        // Act
        var url = provider.GetUrl(root, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void DefaultNested1Level()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);

        var uri = new Uri("https://example.org/page/");

        var provider = new VirtualNodeUrlProvider(umbracoContextAccessor, variationContextAccessor, siteDomainMapper);

        // Act
        var url = provider.GetUrl(page, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/page/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
    }

    [Fact]
    public void DefaultNested2Levels()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = GenerateNode("Home", "home");
        var page = GenerateNode("Page", "page", root);
        var nested = GenerateNode("Nested Page", "nested", page);

        var uri = new Uri("https://example.org/page/nested/");

        var provider = new VirtualNodeUrlProvider(umbracoContextAccessor, variationContextAccessor, siteDomainMapper);

        // Act
        var url = provider.GetUrl(nested, UrlMode.Default, null, uri);

        // Assert
        url.Should().NotBeNull();
        url.Text.Should().Be("/page/nested/");
        url.IsUrl.Should().Be(true);
        url.Culture.Should().BeNull();
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

        var cultures = new Dictionary<string, PublishedCultureInfo>();
        cultures[key] = new PublishedCultureInfo(key, name, segment, DateTime.Now);

        mock.Setup(m => m.Level)
            .Returns(parent?.Level + 1 ?? 1);

        mock.Setup(m => m.Cultures)
            .Returns(cultures);

        mock.Setup(m => m.Parent)
            .Returns(parent);

        return mock.Object;
    }
}