using System;
using FluentAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Routing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests;

public class InvisibleNodeUrlProvider_GetUrl
{
    #region Default URL Mode
    
    [Fact]
    public void Should_Return_DefaultRoot()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
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
    public void Should_Return_DefaultNested1Level()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);

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
    public void Should_Return_DefaultNested2Levels()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);
        var nested = UmbracoTestHelper.GenerateNode(3, "Nested", "nested", page);

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
    public void Should_Return_DefaultInvisible()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);
        var invisible = UmbracoTestHelper.GenerateNode(3, "Invisible", "invisible", page);

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
    public void Should_Return_DefaultNestedHidden()
    {
        // Arrange
        var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);
        var invisible = UmbracoTestHelper.GenerateNode(3, "Invisible", "invisible", page);
        var hidden = UmbracoTestHelper.GenerateNode(4, "Hidden", "hidden", invisible);

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
    public void Should_Return_AbsoluteRoot()
    {
        // Arrange
        var domain = UmbracoTestHelper.GenerateDomain("example.org", 1);
        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
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
    public void Should_Return_AbsoluteNested1Level()
    {
        // Arrange
        var domain = UmbracoTestHelper.GenerateDomain("example.org", 1);
        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);

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
    public void Should_Return_AbsoluteNested2Levels()
    {
        // Arrange
        var domain = UmbracoTestHelper.GenerateDomain("example.org", 1);
        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var rulesManager = new Mock<IInvisibleNodeRulesManager>();

        rulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);
        var nested = UmbracoTestHelper.GenerateNode(3, "Nested", "nested", page);

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
    public void Should_Return_AbsoluteInvisible()
    {
        // Arrange
        var domain = UmbracoTestHelper.GenerateDomain("example.org", 1);
        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);
        var invisible = UmbracoTestHelper.GenerateNode(3, "Invisible", "invisible", page);

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
    public void Should_Return_AbsoluteNestedHidden()
    {
        // Arrange
        var domain = UmbracoTestHelper.GenerateDomain("example.org", 1);
        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(domain.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        var page = UmbracoTestHelper.GenerateNode(2, "Page", "page", root);
        var invisible = UmbracoTestHelper.GenerateNode(3, "Invisible", "invisible", page);
        var hidden = UmbracoTestHelper.GenerateNode(4, "Hidden", "hidden", invisible);

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
}