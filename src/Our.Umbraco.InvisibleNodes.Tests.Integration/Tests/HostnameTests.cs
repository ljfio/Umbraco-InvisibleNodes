// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

// Covers issue #12: Home > News > 2026 (invisible) > Article, with more than one hostname on the home node.
// Only the visible nodes have URLs, and the URLs that include the invisible node are not served.
[Collection("Web")]
public class HostnameTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    private static readonly string[] Hosts = ["https://one.example.org", "https://two.example.org"];

    private readonly TestWebApplicationFactory _factory = factory;

    [Fact]
    public async Task Should_Return_Only_Visible_Urls_For_Each_Hostname()
    {
        using var context = UmbracoContext;

        var (_, invisibleNode, articleNode) = await CreateContent();

        var current = new Uri(Hosts[0]);

        // The page below the invisible node has one URL per hostname
        var article = UmbracoContext.Content.GetById(articleNode.Id);
        article.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(article!, UrlMode.Absolute, current: current)
            .Should()
            .Be("https://one.example.org/news/article/");

        PublishedUrlProvider.GetOtherUrls(articleNode.Id, current)
            .Select(info => info.Url?.ToString())
            .Should()
            .BeEquivalentTo("https://two.example.org/news/article/");

        // The invisible node does not have a URL
        var invisible = UmbracoContext.Content.GetById(invisibleNode.Id);
        invisible.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(invisible!, UrlMode.Absolute, current: current)
            .Should()
            .Be("#");

        PublishedUrlProvider.GetOtherUrls(invisibleNode.Id, current)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Should_Return_Only_Visible_Urls_In_Url_Info()
    {
        using var context = UmbracoContext;

        var (_, invisibleNode, articleNode) = await CreateContent();

        var urlInfoProvider = Services.GetRequiredService<IPublishedUrlInfoProvider>();

        // The URLs listed in the backoffice
        var articleUrls = await urlInfoProvider.GetAllAsync(articleNode);

        // Listed once per culture and hostname, so the same URL can appear more than once
        articleUrls.Select(info => info.Url?.ToString())
            .Distinct()
            .Should()
            .BeEquivalentTo(
                "https://one.example.org/news/article/",
                "https://two.example.org/news/article/");

        var invisibleUrls = await urlInfoProvider.GetAllAsync(invisibleNode);

        invisibleUrls.Where(info => info.Url is not null)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Should_Not_Serve_Urls_Including_Invisible_Node()
    {
        using var context = UmbracoContext;

        await CreateContent();

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        foreach (string host in Hosts)
        {
            // The URL without the invisible node is served
            var response = await client.GetAsync($"{host}/news/article");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await response.Content.ReadAsStringAsync()).Should().Contain("Content Page: Article");

            // The URL with the invisible node redirects to the one without
            var nativeResponse = await client.GetAsync($"{host}/news/2026/article?page=2");
            nativeResponse.StatusCode.Should().Be(HttpStatusCode.MovedPermanently);

            new Uri(new Uri(host), nativeResponse.Headers.Location!)
                .Should()
                .Be(new Uri($"{host}/news/article/?page=2"));

            // The invisible node is not served
            var invisibleResponse = await client.GetAsync($"{host}/news/2026");
            invisibleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// Creates and publishes Home > News > 2026 (invisible) > Article, with the hostnames assigned to Home
    /// </summary>
    private async Task<(IContent News, IContent Invisible, IContent Article)> CreateContent()
    {
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        ContentService.Publish(homeNode, []).Success.Should().BeTrue();

        var newsNode = ContentService.CreateAndSave("News", homeNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(newsNode, []).Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("2026", newsNode, HiddenNode.ModelTypeAlias);
        ContentService.Publish(invisibleNode, []).Success.Should().BeTrue();

        var articleNode = ContentService.CreateAndSave("Article", invisibleNode, ContentPage.ModelTypeAlias);
        ContentService.Publish(articleNode, []).Success.Should().BeTrue();

        var domainResult = await DomainService.UpdateDomainsAsync(homeNode.Key, new DomainsUpdateModel
        {
            Domains = Hosts.Select(host => new DomainModel { IsoCode = "en-US", DomainName = $"{host}/" }),
        });
        domainResult.Success.Should().BeTrue();

        return (newsNode, invisibleNode, articleNode);
    }

    public void Dispose()
    {
        // Cleanup domains
        foreach (var domain in DomainService.GetAll(true))
            DomainService.Delete(domain);

        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}
