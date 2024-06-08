// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class DomainTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public DomainTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Should_Return_Nested()
    {
        var contextFactory = _factory.Services.GetRequiredService<IUmbracoContextFactory>();
        using var context = contextFactory.EnsureUmbracoContext();

        var contentService = _factory.Services.GetRequiredService<IContentService>();
        var domainService = _factory.Services.GetRequiredService<IDomainService>();
        var localizationService = _factory.Services.GetRequiredService<ILocalizationService>();
        var urlProvider = _factory.Services.GetRequiredService<IPublishedUrlProvider>();

        // Content
        var homeNode = contentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = contentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = contentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = contentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = contentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = contentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Languages
        var englishLanguage = localizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = localizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://example.org/en")
        {
            RootContentId = homeNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = domainService.Save(englishDomain);

        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://example.org/da")
        {
            RootContentId = homeNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = domainService.Save(danishDomain);

        danishDomainResult.Success.Should().BeTrue();

        var assigned = domainService.GetAssignedDomains(homeNode.Id, true);

        assigned.Should().HaveCount(2);

        var publishedAssigned = context.UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://example.org/");

        var publishedNode = context.UmbracoContext.Content!.GetById(nestedNode.Id);
        publishedNode.Should().NotBeNull();

        var url = urlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri);
        url.Should().Be("https://example.org/da/content/nested/");
    }

    [Fact]
    public void Should_Return_Unique_HostNames()
    {
        var contextFactory = _factory.Services.GetRequiredService<IUmbracoContextFactory>();
        using var context = contextFactory.EnsureUmbracoContext();

        var contentService = _factory.Services.GetRequiredService<IContentService>();
        var domainService = _factory.Services.GetRequiredService<IDomainService>();
        var localizationService = _factory.Services.GetRequiredService<ILocalizationService>();
        var urlProvider = _factory.Services.GetRequiredService<IPublishedUrlProvider>();

        // Content
        var homeNode = contentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = contentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        var contentNode = contentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = contentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var nestedNode = contentService.Create("Nested", contentNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = contentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Languages
        var englishLanguage = localizationService.GetLanguageByIsoCode("en-US")!;
        var danishLanguage = localizationService.GetLanguageByIsoCode("da-DK")!;

        // Domains
        var englishDomain = new UmbracoDomain("https://en.example.org/")
        {
            RootContentId = homeNode.Id,
            LanguageId = englishLanguage.Id,
        };
        var englishDomainResult = domainService.Save(englishDomain);

        englishDomainResult.Success.Should().BeTrue();

        var danishDomain = new UmbracoDomain("https://da.example.org/")
        {
            RootContentId = homeNode.Id,
            LanguageId = danishLanguage.Id,
        };
        var danishDomainResult = domainService.Save(danishDomain);

        danishDomainResult.Success.Should().BeTrue();

        var assigned = domainService.GetAssignedDomains(homeNode.Id, true);

        assigned.Should().HaveCount(2);

        var publishedAssigned = context.UmbracoContext.Domains!.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Check pages
        var currentUri = new Uri("https://en.example.org/");

        var publishedNode = context.UmbracoContext.Content!.GetById(nestedNode.Id);
        publishedNode.Should().NotBeNull();

        var url = urlProvider.GetUrl(publishedNode!, UrlMode.Absolute, "da-DK", currentUri);
        url.Should().Be("https://da.example.org/content/nested/");
    }

    public void Dispose()
    {
        var contentService = _factory.Services.GetRequiredService<IContentService>();

        foreach (var content in contentService.GetRootContent())
            contentService.Delete(content);
    }
}