// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
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

    public void Dispose()
    {
        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}