// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using FluentAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;
using Umbraco.Cms.Core.Models.PublishedContent;
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
    public void Should_Return_Null()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        var node = _contentCache.Generate("Home", "home");
        
        string path = string.Empty;
        string? culture = null;

        var locator = new InvisibleNodeLocator(variationContextAccessor, mockRulesManager.Object);
        
        // When
        var result = locator.Locate(node, path, culture);

        // Then
        result.Should().BeNull();
    }

    [Fact]
    public void Should_Throw_NullArgumentException()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();
        
        IPublishedContent? node = null;
        string path = "/example/";
        string? culture = null;

        var locator = new InvisibleNodeLocator(variationContextAccessor, mockRulesManager.Object);

        // When
        var act = () => locator.Locate(node, path, culture);

        // Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Return_First_Child()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var node = _contentCache.Generate("Node", "node");
        var home = _contentCache.Generate("Home", "home", children: node.AsEnumerableOfOne());
        
        string path = "/node/";
        string? culture = null;

        var locator = new InvisibleNodeLocator(variationContextAccessor, mockRulesManager.Object);
        
        // When
        var result = locator.Locate(home, path, culture);

        // Then
        result.Should().Be(node);
    }

    [Fact]
    public void Should_Return_Nested_Child()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var nested = _contentCache.Generate("Nested", "nested");
        var node = _contentCache.Generate("Node", "node", children: nested.AsEnumerableOfOne());
        var home = _contentCache.Generate("Home", "home", children: node.AsEnumerableOfOne());
        
        string path = "/node/nested/";
        string? culture = null;

        var locator = new InvisibleNodeLocator(variationContextAccessor, mockRulesManager.Object);
        
        // When
        var result = locator.Locate(home, path, culture);

        // Then
        result.Should().Be(nested);
    }
    
    [Fact]
    public void Should_Return_Child_Same_Name()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsAny<IPublishedContent>()))
            .Returns(false);

        var nested = _contentCache.Generate("Node", "node");
        var node = _contentCache.Generate("Node", "node", children: nested.AsEnumerableOfOne());
        var home = _contentCache.Generate("Home", "home", children: node.AsEnumerableOfOne());
        
        string path = "/node/node/";
        string? culture = null;

        var locator = new InvisibleNodeLocator(variationContextAccessor, mockRulesManager.Object);
        
        // When
        var result = locator.Locate(home, path, culture);

        // Then
        result.Should().Be(nested);
    }

    [Fact]
    public void Should_Return_Hidden_Child()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        
        var node = _contentCache.Generate("Node", "node");
        var hidden = _contentCache.Generate("Hidden", "hidden", children: node.AsEnumerableOfOne());
        var home = _contentCache.Generate("Home", "home", children: hidden.AsEnumerableOfOne());
        
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsIn(hidden)))
            .Returns(true);

        mockRulesManager.Setup(s => s.IsInvisibleNode(It.IsNotIn(hidden)))
            .Returns(false);
        
        string path = "/node/";
        string? culture = null;

        var locator = new InvisibleNodeLocator(variationContextAccessor, mockRulesManager.Object);
        
        // When
        var result = locator.Locate(home, path, culture);

        // Then
        result.Should().Be(node);
    }
}