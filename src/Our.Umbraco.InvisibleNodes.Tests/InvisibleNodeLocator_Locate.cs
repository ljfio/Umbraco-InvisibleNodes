// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using FluentAssertions;
using Moq;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests;

public class InvisibleNodeLocator_Locate
{
    [Fact]
    public void Should_Return_Null()
    {
        // Given
        var variationContextAccessor = new ThreadCultureVariationContextAccessor();
        var mockRulesManager = new Mock<IInvisibleNodeRulesManager>();

        var node = UmbracoTestHelper.GenerateNode(1, "Home", "home");
        
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

        var node = UmbracoTestHelper.GenerateNode(2, "Node", "node");
        var home = UmbracoTestHelper.GenerateNode(1, "Home", "home", children: node.AsEnumerableOfOne());
        
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

        var nested = UmbracoTestHelper.GenerateNode(3, "Nested", "nested");
        var node = UmbracoTestHelper.GenerateNode(2, "Node", "node", children: nested.AsEnumerableOfOne());
        var home = UmbracoTestHelper.GenerateNode(1, "Home", "home", children: node.AsEnumerableOfOne());
        
        string path = "/node/nested/";
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
        
        var node = UmbracoTestHelper.GenerateNode(2, "Node", "node");
        var hidden = UmbracoTestHelper.GenerateNode(2, "Hidden", "hidden", children: node.AsEnumerableOfOne());
        var home = UmbracoTestHelper.GenerateNode(1, "Home", "home", children: hidden.AsEnumerableOfOne());
        
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