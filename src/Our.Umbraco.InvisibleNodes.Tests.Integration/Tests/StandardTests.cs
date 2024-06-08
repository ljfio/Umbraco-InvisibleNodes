// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class StandardTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public StandardTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_Root()
    {
        var contextFactory = _factory.Services.GetRequiredService<IUmbracoContextFactory>();
        using var context = contextFactory.EnsureUmbracoContext();

        var contentService = _factory.Services.GetRequiredService<IContentService>();

        // Content
        var homeNode = contentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = contentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = _factory.CreateClient();

        var httpResponse = await client.GetAsync("/");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Home Page: Home");
    }

    [Fact]
    public async Task Should_Return_Child()
    {
        var contextFactory = _factory.Services.GetRequiredService<IUmbracoContextFactory>();
        using var context = contextFactory.EnsureUmbracoContext();

        var contentService = _factory.Services.GetRequiredService<IContentService>();

        // Content
        var homeNode = contentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = contentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = contentService.Create("Content 1", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = contentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = _factory.CreateClient();

        var httpResponse = await client.GetAsync("/content-1");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Content Page: Content 1");
    }

    [Fact]
    public async Task Should_Return_Nested()
    {
        var contextFactory = _factory.Services.GetRequiredService<IUmbracoContextFactory>();
        using var context = contextFactory.EnsureUmbracoContext();

        var contentService = _factory.Services.GetRequiredService<IContentService>();

        // Content
        var homeNode = contentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = contentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = contentService.Create("Content 2", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = contentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = contentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = contentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = _factory.CreateClient();

        var httpResponse = await client.GetAsync("/content-2/nested");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Content Page: Nested");
    }

    [Fact]
    public async Task Should_Return_Nested_Same_Name()
    {
        var contextFactory = _factory.Services.GetRequiredService<IUmbracoContextFactory>();
        using var context = contextFactory.EnsureUmbracoContext();

        var contentService = _factory.Services.GetRequiredService<IContentService>();

        // Content
        var homeNode = contentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = contentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = contentService.Create("Nested", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = contentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = contentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = contentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = _factory.CreateClient();

        var httpResponse = await client.GetAsync("/nested/nested");
        httpResponse.EnsureSuccessStatusCode();

        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        htmlContent.Should().Contain("Content Page: Nested");
    }

    public void Dispose()
    {
        var contentService = _factory.Services.GetRequiredService<IContentService>();

        foreach (var content in contentService.GetRootContent())
            contentService.Delete(content);
    }
}