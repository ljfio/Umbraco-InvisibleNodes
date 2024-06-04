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

namespace Our.Umbraco.InvisibleNodes.Tests.Integration;

public class StandardTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory = new();

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
        
        var contentNode = contentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = contentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        // Check pages
        using var client = _factory.CreateClient();

        var contentResponse = await client.GetAsync("/content");
        contentResponse.EnsureSuccessStatusCode();

        var content = await contentResponse.Content.ReadAsStringAsync();

        content.Should().Contain("Content Page: Content");
    }

    public void Dispose() => _factory.Dispose();
}