// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using FluentAssertions;
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
}