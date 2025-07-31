// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class DomainTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Fact]
    public async Task Should_Return_Nested()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.Publish(homeNode, []);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.CreateAndSave("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.Publish(contentNode, []);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.CreateAndSave("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.Publish(nestedNode, []);

        nestedPublishResult.Success.Should().BeTrue();

        // Domains
        var englishDomain = new DomainModel
        {
            IsoCode = "en-US",
            DomainName = "https://example.org/en",
        };
        
        var danishDomain = new DomainModel
        {
            IsoCode = "da-DK",
            DomainName = "https://example.org/da"
        };

        var updateDomainsResult = await DomainService.UpdateDomainsAsync(homeNode.Key, new DomainsUpdateModel
        {
            Domains = [englishDomain, danishDomain],
        });
        updateDomainsResult.Success.Should().BeTrue();

        var assigned = await DomainService.GetAssignedDomainsAsync(homeNode.Key, true);
        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains.GetAssigned(homeNode.Id, true);
        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://example.org/");

        var publishedNode = UmbracoContext.Content.GetById(nestedNode.Id);
        publishedNode.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri)
            .Should()
            .Be("https://example.org/da/content/nested/");
    }

    [Fact]
    public async Task Should_Return_Hidden()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.Publish(homeNode, []);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.CreateAndSave("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.Publish(contentNode, []);

        contentPublishResult.Success.Should().BeTrue();

        var invisibleNode = ContentService.CreateAndSave("Invisible", contentNode, HiddenNode.ModelTypeAlias);
        var hiddenNodeResult = ContentService.Publish(invisibleNode, []);

        hiddenNodeResult.Success.Should().BeTrue();

        var hiddenNode = ContentService.CreateAndSave("Hidden", invisibleNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.Publish(hiddenNode, []);

        nestedPublishResult.Success.Should().BeTrue();
        
        // Domains
        var englishDomain = new DomainModel
        {
            IsoCode = "en-US",
            DomainName = "https://example.org/en",
        };

        var danishDomain = new DomainModel
        {
            IsoCode = "da-DK",
            DomainName = "https://example.org/da",
        };
        
        var updateDomainsResult = await DomainService.UpdateDomainsAsync(homeNode.Key , new  DomainsUpdateModel
        {
            Domains = [englishDomain, danishDomain],
        });
        updateDomainsResult.Success.Should().BeTrue();

        var assigned = await DomainService.GetAssignedDomainsAsync(homeNode.Key, true);
        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://example.org/");

        var publishedNode = UmbracoContext.Content.GetById(hiddenNode.Id);
        publishedNode.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "en-US", currentUri)
            .Should()
            .Be("https://example.org/en/content/hidden/");

        PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri)
            .Should()
            .Be("https://example.org/da/content/hidden/");
    }

    [Fact]
    public async Task Should_Return_Variant()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.Publish(homeNode, []);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.CreateAndSave("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.Publish(contentNode, []);

        contentPublishResult.Success.Should().BeTrue();
        
        // Domains
        var englishDomain = new DomainModel
        {
            IsoCode = "en-US",
            DomainName = "https://example.org/",
        };

        var danishDomain = new DomainModel
        {
            IsoCode = "da-DK",
            DomainName = "https://example.org/da",
        };

        var updateDomainsResult = await DomainService.UpdateDomainsAsync(homeNode.Key, new DomainsUpdateModel
        {
            Domains = [englishDomain, danishDomain],
        });
        updateDomainsResult.Success.Should().BeTrue();

        var assigned = await DomainService.GetAssignedDomainsAsync(homeNode.Key, true);
        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check node URLs
        var publishedNode = UmbracoContext.Content.GetById(contentNode.Id);
        publishedNode.Should().NotBeNull();

        var current = new Uri("http://example.org/test");

        publishedNode!.Url(PublishedUrlProvider, culture: "en-US")
            .Should()
            .Be("https://example.org/content/");
        publishedNode!.Url(PublishedUrlProvider, culture: "en-US", mode: UrlMode.Absolute)
            .Should()
            .Be("https://example.org/content/");

        PublishedUrlProvider.GetUrl(publishedNode!, culture: "en-US", current: current)
            .Should()
            .Be("/content/");
        PublishedUrlProvider.GetUrl(publishedNode!, culture: "en-US", mode: UrlMode.Absolute)
            .Should()
            .Be("https://example.org/content/");

        PublishedUrlProvider.GetUrl(publishedNode!, culture: "da-DK", current: current)
            .Should()
            .Be("/da/content/");
        PublishedUrlProvider.GetUrl(publishedNode!, culture: "da-DK", mode: UrlMode.Absolute)
            .Should()
            .Be("https://example.org/da/content/");
    }

    [Fact]
    public async Task Should_Return_Unique_HostNames()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.Publish(homeNode, []);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.CreateAndSave("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.Publish(contentNode, []);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.CreateAndSave("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.Publish(nestedNode, []);

        nestedPublishResult.Success.Should().BeTrue();


        // Domains
        var englishDomain = new DomainModel
        {
            IsoCode = "en-US",
            DomainName = "https://en.example.org/",
        };
        
        var danishDomain = new DomainModel
        {
            IsoCode = "da-DK",
            DomainName = "https://da.example.org/",
        };
        
        var danishDomainResult = await DomainService.UpdateDomainsAsync(homeNode.Key, new DomainsUpdateModel
        {
            Domains = [englishDomain, danishDomain],
        });
        danishDomainResult.Success.Should().BeTrue();

        var assigned = await DomainService.GetAssignedDomainsAsync(homeNode.Key, true);
        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains.GetAssigned(homeNode.Id, true);
        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://en.example.org/");

        var publishedNode = UmbracoContext.Content.GetById(nestedNode.Id);
        publishedNode.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri)
            .Should()
            .Be("https://da.example.org/content/nested/");
    }

    [Fact]
    public async Task Should_Return_Unique_Home()
    {
        using var context = UmbracoContext;

        // Content
        var firstNode = ContentService.CreateAndSave("Home 1", Constants.System.Root, HomePage.ModelTypeAlias);
        var firstPublishResult = ContentService.Publish(firstNode, []);
        firstPublishResult.Success.Should().BeTrue();

        var secondNode = ContentService.CreateAndSave("Home 2", Constants.System.Root, HomePage.ModelTypeAlias);
        var secondPublishResult = ContentService.Publish(secondNode, []);
        secondPublishResult.Success.Should().BeTrue();
        
        // Domains
        var englishDomain = new DomainModel
        {
            IsoCode = "en-US",
            DomainName = "https://en.example.org/",
        };
        
        var englishDomainResult = await DomainService.UpdateDomainsAsync(firstNode.Key, new DomainsUpdateModel
        {
            Domains = [englishDomain],
        });
        
        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new DomainModel
        {
            IsoCode = "da-DK",
            DomainName = "https://da.example.org/",
        };
        var danishDomainResult = await DomainService.UpdateDomainsAsync(secondNode.Key, new DomainsUpdateModel
        {
            Domains = [danishDomain],
        });
        danishDomainResult.Success.Should().BeTrue();

        var firstDomains = await DomainService.GetAssignedDomainsAsync(firstNode.Key, true);
        firstDomains.Should().HaveCount(1);

        var firstPublishedDomains = UmbracoContext.Domains.GetAssigned(firstNode.Id, true);
        firstPublishedDomains.Should().HaveCount(1);

        var secondDomains = await DomainService.GetAssignedDomainsAsync(secondNode.Key, true);
        secondDomains.Should().HaveCount(1);

        var secondPublishedDomains = UmbracoContext.Domains.GetAssigned(secondNode.Id, true);
        secondPublishedDomains.Should().HaveCount(1);

        // Check URLs
        var englishUri = new Uri("https://en.example.org");
        var danishUri = new Uri("https://da.example.org");

        var firstPublishedNode = UmbracoContext.Content.GetById(firstNode.Id);
        firstPublishedNode.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(firstPublishedNode!)
            .Should()
            .Be("https://en.example.org/");
        PublishedUrlProvider.GetUrl(firstPublishedNode!, mode: UrlMode.Absolute, current: englishUri)
            .Should()
            .Be("https://en.example.org/");
        PublishedUrlProvider.GetUrl(firstPublishedNode!, current: englishUri)
            .Should()
            .Be("/");
        PublishedUrlProvider.GetUrl(firstPublishedNode!, current: danishUri)
            .Should()
            .Be("https://en.example.org/");

        var secondPublishedNode = UmbracoContext.Content.GetById(secondNode.Id);
        secondPublishedNode.Should().NotBeNull();

        PublishedUrlProvider.GetUrl(secondPublishedNode!)
            .Should()
            .Be("https://da.example.org/");
        PublishedUrlProvider.GetUrl(secondPublishedNode!, mode: UrlMode.Absolute, current: englishUri)
            .Should()
            .Be("https://da.example.org/");
        PublishedUrlProvider.GetUrl(secondPublishedNode!, current: englishUri)
            .Should()
            .Be("https://da.example.org/");
        PublishedUrlProvider.GetUrl(secondPublishedNode!, current: danishUri)
            .Should()
            .Be("/");
    }

    public void Dispose()
    {
        // Cleanup domains
        foreach (var content in DomainService.GetAll(true))
            DomainService.Delete(content);

        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);
    }
}