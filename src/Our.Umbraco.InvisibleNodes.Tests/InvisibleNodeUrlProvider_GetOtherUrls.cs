// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using FluentAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests;

public class InvisibleNodeUrlProvider_GetOtherUrls
{
    [Fact]
    public void Should_Return_EmptyForMatchingRoot()
    {
        // Arrange
        var domains = UmbracoTestHelper.GenerateDomains("example.org");

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "");

        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(
            domains: domains,
            content: root.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            mockRulesManager.Object);

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
        var domains = UmbracoTestHelper.GenerateDomains("example.org", "example.com");

        var root = UmbracoTestHelper.GenerateNode(1, "Home", "");

        var umbracoContextAccessor = UmbracoTestHelper.GenerateUmbracoContextAccessor(
            domains: domains,
            content: root.AsEnumerableOfOne());

        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var siteDomainMapper = new SiteDomainMapper();

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager
            .Setup(m => m.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var uri = new Uri("https://example.org/");

        var provider = new InvisibleNodeUrlProvider(
            umbracoContextAccessor,
            variationContextAccessor,
            siteDomainMapper,
            mockRulesManager.Object);

        // Act
        var urls = provider.GetOtherUrls(root.Id, uri);

        // Assert
        urls.Should().NotBeNullOrEmpty();
        urls.Should().ContainSingle(value => value.Text == "https://example.com/");
    }

}