// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

// The InvisibleNodes:NativeUrls setting decides what happens to a URL that includes an invisible node's segment.
// The redirect, which is the default, is covered by HostnameTests.
[Collection("Web")]
public class NativeUrlTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    private const string NativeUrlsKey = "InvisibleNodes:NativeUrls";

    [Fact]
    public async Task Should_Not_Find_Native_Url_When_NotFound()
    {
        using var context = UmbracoContext;

        SetConfiguration(NativeUrlsKey, "NotFound");
        CreateContent();

        using var client = NonRedirectingHttpClient;

        (await client.GetAsync("/news/article/")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/news/2026/article/")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync("/news/2026/")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Serve_Native_Url_When_Allowed()
    {
        using var context = UmbracoContext;

        SetConfiguration(NativeUrlsKey, "Allow");
        CreateContent();

        using var client = NonRedirectingHttpClient;

        (await client.GetAsync("/news/article/")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Left for Umbraco to serve
        var response = await client.GetAsync("/news/2026/article/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        (await response.Content.ReadAsStringAsync()).Should().Contain("Content Page: Article");
    }

    /// <summary>
    /// Creates and publishes Home > News > 2026 (invisible) > Article
    /// </summary>
    private void CreateContent()
    {
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        ContentService.Publish(homeNode, []).Success.Should().BeTrue();

        var newsNode = ContentService.CreateAndSave("News", homeNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(newsNode, []).Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("2026", newsNode, HiddenNode.ModelTypeAlias);
        ContentService.Publish(invisibleNode, []).Success.Should().BeTrue();

        var articleNode = ContentService.CreateAndSave("Article", invisibleNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(articleNode, []).Success.Should().BeTrue();
    }

    public void Dispose()
    {
        // Back to the default for the other tests
        SetConfiguration(NativeUrlsKey, "Redirect");

        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}
