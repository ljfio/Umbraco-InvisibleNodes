// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using FluentAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class DomainTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Fact]
    public void Should_Return_Nested()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Languages
        var englishLanguage = LocalizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = LocalizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://example.org/en")
        {
            RootContentId = homeNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = DomainService.Save(englishDomain);

        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://example.org/da")
        {
            RootContentId = homeNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = DomainService.Save(danishDomain);

        danishDomainResult.Success.Should().BeTrue();

        var assigned = DomainService.GetAssignedDomains(homeNode.Id, true);

        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://example.org/");

        var publishedNode = UmbracoContext.Content!.GetById(nestedNode.Id);
        publishedNode.Should().NotBeNull();

        var url = PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri);
        url.Should().Be("https://example.org/da/content/nested/");
    }

    [Fact]
    public void Should_Return_Hidden()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var invisibleNode = ContentService.Create("Invisible", contentNode, HiddenNode.ModelTypeAlias);
        var hiddenNodeResult = ContentService.SaveAndPublish(invisibleNode);

        hiddenNodeResult.Success.Should().BeTrue();

        var hiddenNode = ContentService.Create("Hidden", invisibleNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(hiddenNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Languages
        var englishLanguage = LocalizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = LocalizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://example.org/en")
        {
            RootContentId = homeNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = DomainService.Save(englishDomain);

        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://example.org/da")
        {
            RootContentId = homeNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = DomainService.Save(danishDomain);

        danishDomainResult.Success.Should().BeTrue();

        var assigned = DomainService.GetAssignedDomains(homeNode.Id, true);

        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://example.org/");

        var publishedNode = UmbracoContext.Content!.GetById(hiddenNode.Id);
        publishedNode.Should().NotBeNull();

        var englishUrl = PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "en-US", currentUri);
        englishUrl.Should().Be("https://example.org/en/content/hidden/");

        var danishUrl = PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri);
        danishUrl.Should().Be("https://example.org/da/content/hidden/");
    }

    [Fact]
    public void Should_Return_Variant()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        // Languages
        var englishLanguage = LocalizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = LocalizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://example.org/")
        {
            RootContentId = homeNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = DomainService.Save(englishDomain);

        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://example.org/da")
        {
            RootContentId = homeNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = DomainService.Save(danishDomain);

        danishDomainResult.Success.Should().BeTrue();

        var assigned = DomainService.GetAssignedDomains(homeNode.Id, true);

        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check node URLs
        var publishedNode = UmbracoContext.Content!.GetById(contentNode.Id);
        publishedNode.Should().NotBeNull();

        publishedNode!.Url(PublishedUrlProvider, "en-US").Should().Be("/content/");
        publishedNode!.Url(PublishedUrlProvider, "en-US", UrlMode.Absolute).Should().Be("https://example.org/content/");

        publishedNode!.Url(PublishedUrlProvider, "da-DK").Should().Be("/da/content/");
        publishedNode!.Url(PublishedUrlProvider, "da-DK", UrlMode.Absolute).Should()
            .Be("https://example.org/da/content/");
    }

    [Fact]
    public void Should_Return_Unique_HostNames()
    {
        using var context = UmbracoContext;

        // Content
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = ContentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Languages
        var englishLanguage = LocalizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = LocalizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://en.example.org/")
        {
            RootContentId = homeNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = DomainService.Save(englishDomain);

        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://da.example.org/")
        {
            RootContentId = homeNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = DomainService.Save(danishDomain);

        danishDomainResult.Success.Should().BeTrue();

        var assigned = DomainService.GetAssignedDomains(homeNode.Id, true);

        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://en.example.org/");

        var publishedNode = UmbracoContext.Content!.GetById(nestedNode.Id);
        publishedNode.Should().NotBeNull();

        var url = PublishedUrlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri);
        url.Should().Be("https://da.example.org/content/nested/");
    }

    [Fact]
    public void Should_Return_Unique_Home_Domain()
    {
        using var context = UmbracoContext;

        // Content
        var firstNode = ContentService.Create("Home 1", Constants.System.Root, HomePage.ModelTypeAlias);
        var firstPublishResult = ContentService.SaveAndPublish(firstNode);
        firstPublishResult.Success.Should().BeTrue();
        
        var secondNode = ContentService.Create("Home 2", Constants.System.Root, HomePage.ModelTypeAlias);
        var secondPublishResult = ContentService.SaveAndPublish(secondNode);
        secondPublishResult.Success.Should().BeTrue();
        
        // Languages
        var englishLanguage = LocalizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = LocalizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://en.example.org/")
        {
            RootContentId = firstNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = DomainService.Save(englishDomain);
        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://da.example.org/")
        {
            RootContentId = secondNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = DomainService.Save(danishDomain);
        danishDomainResult.Success.Should().BeTrue();
        
        var firstDomains = DomainService.GetAssignedDomains(firstNode.Id, true);
        firstDomains.Should().HaveCount(1);

        var firstPublishedDomains = UmbracoContext.Domains!.GetAssigned(firstNode.Id, true);
        firstPublishedDomains.Should().HaveCount(1);
        
        var secondDomains = DomainService.GetAssignedDomains(secondNode.Id, true);
        secondDomains.Should().HaveCount(1);

        var secondPublishedDomains = UmbracoContext.Domains!.GetAssigned(secondNode.Id, true);
        secondPublishedDomains.Should().HaveCount(1);

        // Check URLs
        var currentUri = new Uri("https://en.example.org");

        var firstPublishedNode = UmbracoContext.Content!.GetById(firstNode.Id);
        firstPublishedNode.Should().NotBeNull();

        var firstUrl = PublishedUrlProvider.GetUrl(firstPublishedNode!, mode: UrlMode.Absolute, current: currentUri);

        firstUrl.Should().Be("https://en.example.org/");
        
        var secondPublishedNode = UmbracoContext.Content!.GetById(secondNode.Id);
        secondPublishedNode.Should().NotBeNull();
        
        var secondUrl = PublishedUrlProvider.GetUrl(secondPublishedNode!, mode: UrlMode.Absolute, current: currentUri);
        secondUrl.Should().Be("https://da.example.org/");
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