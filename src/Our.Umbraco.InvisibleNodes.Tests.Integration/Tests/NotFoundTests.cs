// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

// Home > News > 2026 (invisible) > Article, with a draft page below the invisible node.
// Only the URLs of published, visible nodes are served.
[Collection("Web")]
public class NotFoundTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Theory]
    [InlineData("/does-not-exist/")]
    [InlineData("/news/missing/")]
    [InlineData("/news/articles/")]
    [InlineData("/news/article/missing/")]
    [InlineData("/news/2026/missing/")]
    [InlineData("/article/")]
    [InlineData("/2026/article/")]
    [InlineData("/news/draft/")]
    [InlineData("/news/2026/draft/")]
    public async Task Should_Not_Find_Content(string path)
    {
        using var context = UmbracoContext;

        CreateContent();

        using var client = NonRedirectingHttpClient;

        // The page that does exist is served, so the content is routable
        var response = await client.GetAsync("/news/article/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.GetAsync(path)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Find_Content_Once_Published()
    {
        using var context = UmbracoContext;

        var draftNode = CreateContent();

        using var client = NonRedirectingHttpClient;

        (await client.GetAsync("/news/draft/")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        ContentService.Publish(draftNode, []).Success.Should().BeTrue();

        var response = await client.GetAsync("/news/draft/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        (await response.Content.ReadAsStringAsync()).Should().Contain("Content Page: Draft");
    }

    /// <summary>
    /// Creates Home > News > 2026 (invisible) > Article and an unpublished Draft, and returns the draft
    /// </summary>
    private IContent CreateContent()
    {
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        ContentService.Publish(homeNode, []).Success.Should().BeTrue();

        var newsNode = ContentService.CreateAndSave("News", homeNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(newsNode, []).Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("2026", newsNode, HiddenNode.ModelTypeAlias);
        ContentService.Publish(invisibleNode, []).Success.Should().BeTrue();

        var articleNode = ContentService.CreateAndSave("Article", invisibleNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(articleNode, []).Success.Should().BeTrue();

        return ContentService.CreateAndSave("Draft", invisibleNode, ContentPage.ModelTypeAlias);
    }

    public void Dispose()
    {
        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}
