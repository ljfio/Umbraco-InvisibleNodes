// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using FluentAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class OtherUrlTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Fact]
    public void Should_Return_Valid_Hidden_Nodes()
    {
        using var context = UmbracoContext;

        // Create Home node to assign domains
        var homeNode = ContentService.Create("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.SaveAndPublish(homeNode);

        homePublishResult.Success.Should().BeTrue();

        // Languages for domains
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

        // Create the other content
        var contentNode = ContentService.Create("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.SaveAndPublish(contentNode);

        contentPublishResult.Success.Should().BeTrue();

        var hiddenNode = ContentService.Create("Hidden", contentNode, HiddenNode.ModelTypeAlias);
        var hiddenPublishResult = ContentService.SaveAndPublish(hiddenNode);

        hiddenPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.Create("Nested", hiddenNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.SaveAndPublish(nestedNode);

        nestedPublishResult.Success.Should().BeTrue();

        // Check other URLs
        var englishUri = new Uri("https://en.example.org/");
        var englishOtherUrls = PublishedUrlProvider.GetOtherUrls(nestedNode.Id, englishUri).ToArray();

        englishOtherUrls.Should()
            .Contain(url => url.Text
                .Equals("https://da.example.org/content/nested/", StringComparison.InvariantCultureIgnoreCase));

        var danishUri = new Uri("https://da.example.org/");
        var danishOtherUrls = PublishedUrlProvider.GetOtherUrls(nestedNode.Id, danishUri).ToArray();

        danishOtherUrls.Should()
            .Contain(url => url.Text
                .Equals("https://en.example.org/content/nested/", StringComparison.InvariantCultureIgnoreCase));
    }

    public void Dispose()
    {
        // Cleanup content
        foreach (var content in ContentService.GetRootContent())
            ContentService.Delete(content);

        // Cleanup domains
        foreach (var domain in DomainService.GetAll(true))
            DomainService.Delete(domain);
    }
}