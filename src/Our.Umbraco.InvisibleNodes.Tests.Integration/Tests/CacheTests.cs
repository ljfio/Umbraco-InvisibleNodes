// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

// Each test requests a page first so its route is cached, then changes the content or hostnames
// and checks the cached route is no longer served. Umbraco redirects the routes of renamed and moved nodes.
[Collection("Web")]
public class CacheTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Fact]
    public async Task Should_Clear_Route_When_Renamed()
    {
        using var context = UmbracoContext;

        var (_, _, page) = CreateContent("Old Name");

        using var client = HttpClient;

        (await client.GetAsync("/old-name")).StatusCode.Should().Be(HttpStatusCode.OK);

        page.Name = "New Name";
        ContentService.Save(page);
        ContentService.Publish(page, []).Success.Should().BeTrue();

        await ShouldRedirect("/old-name", "/new-name/");
        (await client.GetAsync("/new-name")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Clear_Routes_Below_When_Ancestor_Renamed()
    {
        using var context = UmbracoContext;

        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        ContentService.Publish(homeNode, []).Success.Should().BeTrue();

        var sectionNode = ContentService.CreateAndSave("Old Section", homeNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(sectionNode, []).Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("Invisible", sectionNode, HiddenNode.ModelTypeAlias);
        ContentService.Publish(invisibleNode, []).Success.Should().BeTrue();

        var pageNode = ContentService.CreateAndSave("Page", invisibleNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(pageNode, []).Success.Should().BeTrue();

        using var client = HttpClient;

        (await client.GetAsync("/old-section/page")).StatusCode.Should().Be(HttpStatusCode.OK);

        sectionNode.Name = "New Section";
        ContentService.Save(sectionNode);
        ContentService.Publish(sectionNode, []).Success.Should().BeTrue();

        await ShouldRedirect("/old-section/page", "/new-section/page/");
        (await client.GetAsync("/new-section/page")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Clear_Route_When_Moved()
    {
        using var context = UmbracoContext;

        var (homeNode, _, page) = CreateContent("Page");

        var sectionNode = ContentService.CreateAndSave("Section", homeNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(sectionNode, []).Success.Should().BeTrue();

        using var client = HttpClient;

        (await client.GetAsync("/page")).StatusCode.Should().Be(HttpStatusCode.OK);

        ContentService.Move(page, sectionNode.Id).Success.Should().BeTrue();

        await ShouldRedirect("/page", "/section/page/");
        (await client.GetAsync("/section/page")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Clear_Route_When_Invisible_Parent_Unpublished()
    {
        using var context = UmbracoContext;

        var (_, invisibleNode, _) = CreateContent("Page");

        using var client = HttpClient;

        (await client.GetAsync("/page")).StatusCode.Should().Be(HttpStatusCode.OK);

        ContentService.Unpublish(invisibleNode).Success.Should().BeTrue();

        (await client.GetAsync("/page")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Clear_Route_When_Trashed()
    {
        using var context = UmbracoContext;

        var (_, invisibleNode, _) = CreateContent("Page");

        using var client = HttpClient;

        (await client.GetAsync("/page")).StatusCode.Should().Be(HttpStatusCode.OK);

        ContentService.MoveToRecycleBin(invisibleNode).Success.Should().BeTrue();

        (await client.GetAsync("/page")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Clear_Route_When_Hostname_Assigned_Below_Home()
    {
        using var context = UmbracoContext;

        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        ContentService.Publish(homeNode, []).Success.Should().BeTrue();

        var sectionNode = ContentService.CreateAndSave("Section", homeNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(sectionNode, []).Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("Invisible", sectionNode, HiddenNode.ModelTypeAlias);
        ContentService.Publish(invisibleNode, []).Success.Should().BeTrue();

        var pageNode = ContentService.CreateAndSave("Page", invisibleNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(pageNode, []).Success.Should().BeTrue();

        using var client = HttpClient;

        (await client.GetAsync("https://example.org/section/page")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Assigning the hostname to the section makes it the root for that host
        var domainResult = await DomainService.UpdateDomainsAsync(sectionNode.Key, new DomainsUpdateModel
        {
            Domains = [new DomainModel { IsoCode = "en-US", DomainName = "https://example.org/" }],
        });
        domainResult.Success.Should().BeTrue();

        (await client.GetAsync("https://example.org/section/page")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync("https://example.org/page")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Checks the old route is no longer served, as Umbraco redirects it to the new route
    /// </summary>
    private async Task ShouldRedirect(string oldPath, string newPath)
    {
        using var client = NonRedirectingHttpClient;

        var response = await client.GetAsync(oldPath);

        response.StatusCode.Should().Be(HttpStatusCode.MovedPermanently);
        response.Headers.Location?.ToString().Should().EndWith(newPath);
    }

    /// <summary>
    /// Creates and publishes Home > Invisible > page
    /// </summary>
    private (IContent Home, IContent Invisible, IContent Page) CreateContent(string pageName)
    {
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        ContentService.Publish(homeNode, []).Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("Invisible", homeNode, HiddenNode.ModelTypeAlias);
        ContentService.Publish(invisibleNode, []).Success.Should().BeTrue();

        var pageNode = ContentService.CreateAndSave(pageName, invisibleNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(pageNode, []).Success.Should().BeTrue();

        return (homeNode, invisibleNode, pageNode);
    }

    public void Dispose()
    {
        ClearDomains();

        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}
