// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class StandardTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Fact]
    public async Task Should_Return_Root()
    {
        using var context = UmbracoContext;
        
        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = HttpClient;

        var httpResponse = await client.GetAsync("/");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Home Page: Home");
    }

    [Fact]
    public async Task Should_Return_Child()
    {
        using var context = UmbracoContext;
        
        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Content 1", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = HttpClient;

        var httpResponse = await client.GetAsync("/content-1");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Content Page: Content 1");
    }

    [Fact]
    public async Task Should_Return_Nested()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Content 2", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = HttpClient;

        var httpResponse = await client.GetAsync("/content-2/nested");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Content Page: Nested");
    }

    [Fact]
    public async Task Should_Return_Nested_Same_Name()
    {
        using var context = UmbracoContext;
        
        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Nested", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = HttpClient;

        var httpResponse = await client.GetAsync("/nested/nested");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Content Page: Nested");
    }

    [Fact]
    public void Should_Return_Nested_Url()
    {
        using var context = UmbracoContext;
        
        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Nested", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        var publishedContent = context.Content!.GetById(nestedNode.Id);

        var current = new Uri("http://example.org");

        PublishedUrlProvider.GetUrl(publishedContent!)
            .Should()
            .Be("/nested/nested/");
        PublishedUrlProvider.GetUrl(publishedContent!, mode: UrlMode.Absolute)
            .Should()
            .Be("http://localhost/nested/nested/");
        PublishedUrlProvider.GetUrl(publishedContent!, mode: UrlMode.Absolute, current: current)
            .Should()
            .Be("http://example.org/nested/nested/");
    }
    
    [Fact]
    public void Should_Return_Second_Node()
    {
        using var context = UmbracoContext;
        
        // Content
        var firstNode = ContentService.Create("First", Constants.System.Root, HomePage.ModelTypeAlias);
        var firstPublishResult = ContentService.SaveAndPublish(firstNode);

        firstPublishResult.Success.Should().BeTrue();
        
        var secondNode = ContentService.Create("Second", Constants.System.Root, HomePage.ModelTypeAlias);
        var secondPublishResult = ContentService.SaveAndPublish(secondNode);

        secondPublishResult.Success.Should().BeTrue();

        var firstPublishedContent = context.Content!.GetById(firstNode.Id);
        var secondPublishedContent = context.Content!.GetById(secondNode.Id);

        var current = new Uri("http://example.org");

        PublishedUrlProvider.GetUrl(firstPublishedContent!)
            .Should()
            .Be("/");
        PublishedUrlProvider.GetUrl(firstPublishedContent!, mode: UrlMode.Absolute)
            .Should()
            .Be("http://localhost/");
        PublishedUrlProvider.GetUrl(firstPublishedContent!, mode: UrlMode.Absolute, current: current)
            .Should()
            .Be("http://example.org/");
        PublishedUrlProvider.GetUrl(secondPublishedContent!)
            .Should()
            .Be("/second/");
        PublishedUrlProvider.GetUrl(secondPublishedContent!, mode: UrlMode.Absolute)
            .Should()
            .Be("http://localhost/second/");
        PublishedUrlProvider.GetUrl(secondPublishedContent!, mode: UrlMode.Absolute, current: current)
            .Should()
            .Be("http://example.org/second/");
    }

    public void Dispose()
    {
        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}