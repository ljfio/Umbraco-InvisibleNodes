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
public class DomainTests : IntegrationTestBase, IDisposable
{
    public DomainTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

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
}