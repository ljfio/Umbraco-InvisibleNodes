// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit;

public class InvisibleNodeLocator_Locate
{
    private readonly FakePublishedContentCache _contentCache;

    public InvisibleNodeLocator_Locate()
    {
        _contentCache = new FakePublishedContentCache();
    }

    [Fact]
    public async Task Should_Return_Null()
    {
        // Given
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        var node = _contentCache.Generate("Home", "home");

        string path = string.Empty;
        string culture = string.Empty;

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, node, path, culture);

        // Then
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_Throw_NullArgumentException()
    {
        // Given
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        IPublishedContent? node = null;
        string path = "/example/";
        string culture = string.Empty;

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var act = async () => await locator.Locate(_contentCache, node!, path, culture);

        // Then
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_Return_First_Child()
    {
        // Given
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var home = _contentCache.Generate("Home", "home");
        var node = _contentCache.Generate("Node", "node", parent: home);

        string path = "/node/";
        string culture = string.Empty;

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, culture);

        // Then
        result.Should().Be(node);
    }

    [Fact]
    public async Task Should_Return_Nested_Child()
    {
        // Given
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var home = _contentCache.Generate("Home", "home");
        var node = _contentCache.Generate("Node", "node", parent: home);
        var nested = _contentCache.Generate("Nested", "nested", parent: node);

        string path = "/node/nested/";
        string culture = string.Empty;

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, culture);

        // Then
        result.Should().Be(nested);
    }

    [Fact]
    public async Task Should_Return_Child_Same_Name()
    {
        // Given
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var home = _contentCache.Generate("Home", "home");
        var node = _contentCache.Generate("Node", "node", parent: home);
        var nested = _contentCache.Generate("Node", "node", parent: node);

        string path = "/node/node/";
        string culture = string.Empty;

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, culture);

        // Then
        result.Should().Be(nested);
    }

    [Fact]
    public async Task Should_Return_Hidden_Child()
    {
        // Given
        var home = _contentCache.Generate("Home", "home");
        var hidden = _contentCache.Generate("Hidden", "hidden", parent: home);
        var node = _contentCache.Generate("Node", "node", parent: hidden);

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsIn(hidden)))
            .Returns(true);

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsNotIn(hidden)))
            .Returns(false);

        string path = "/node/";
        string culture = string.Empty;

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, culture);

        // Then
        result.Should().Be(node);
    }

    [Theory]
    [InlineData("/hidden")]
    [InlineData("/hidden/node")]
    public async Task Should_Return_Null_For_Hidden_Segment(string path)
    {
        // Given
        var home = _contentCache.Generate("Home", "home");
        var hidden = _contentCache.Generate("Hidden", "hidden", parent: home);
        _contentCache.Generate("Node", "node", parent: hidden);

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsIn(hidden)))
            .Returns(true);

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsNotIn(hidden)))
            .Returns(false);

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, string.Empty);

        // Then
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("/node", "node")]
    [InlineData("/nested", "nested")]
    [InlineData("/node/child", "child")]
    [InlineData("/sibling", "sibling")]
    [InlineData("/hidden", null)]
    [InlineData("/missing", null)]
    public async Task Should_Locate_By_Content_Type(string path, string? expected)
    {
        // Given
        var home = _contentCache.Generate("Home", "home");
        _contentCache.Generate("Sibling", "page", parent: home);
        var hidden = _contentCache.Generate("Hidden", "hidden", parent: home);
        var node = _contentCache.Generate("Node", "page", parent: hidden);
        _contentCache.Generate("Child", "page", parent: node);
        var nestedHidden = _contentCache.Generate("Nested Hidden", "hidden", parent: hidden);
        _contentCache.Generate("Nested", "page", parent: nestedHidden);

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.InvisibleContentTypes)
            .Returns(["hidden"]);

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, string.Empty);

        // Then
        result?.Name.ToLowerInvariant().Should().Be(expected);

        if (expected is null)
            result.Should().BeNull();

        // The content types are enough, so each node's content isn't checked
        mockRulesManager.Verify(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()), Times.Never);
    }

    [Theory]
    [InlineData(false, "/page")]
    [InlineData(false, "/draft")]
    [InlineData(true, "/page")]
    [InlineData(true, "/draft")]
    public async Task Should_Not_Return_Unpublished_Child(bool byContentType, string path)
    {
        // Given the unpublished nodes are in the navigation but not in the published cache
        var home = _contentCache.Generate("Home", "home");
        var hidden = _contentCache.Generate("Hidden", "hidden", parent: home);
        _contentCache.Generate("Page", "page", parent: hidden, preview: true);
        _contentCache.Generate("Draft", "page", parent: home, preview: true);

        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        if (byContentType)
            mockRulesManager.Setup(s => s.InvisibleContentTypes).Returns(["hidden"]);
        else
            mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
                .Returns<IPublishedContent>(content => content == hidden);

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            mockRulesManager.Object);

        // When
        var result = await locator.Locate(_contentCache, home, path, string.Empty);

        // Then
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return_Null_When_Node_Has_No_Children()
    {
        // Given
        var home = _contentCache.Generate("Home", "home");

        var locator = new InvisibleNodeLocator(
            _contentCache.DocumentNavigationQueryService,
            _contentCache.DocumentUrlService,
            Mock.Of<IInvisibleNodeRulesManager>());

        // When
        var result = await locator.Locate(_contentCache, home, "/page", string.Empty);

        // Then
        result.Should().BeNull();
    }
}
