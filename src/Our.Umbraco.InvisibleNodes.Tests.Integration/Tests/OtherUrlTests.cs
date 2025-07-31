// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

[Collection("Web")]
public class OtherUrlTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory), IDisposable
{
    [Fact]
    public async Task Should_Return_Valid_Hidden_Nodes()
    {
        using var context = UmbracoContext;

        // Create Home node to assign domains
        var homeNode = ContentService.CreateAndSave("Home", Constants.System.Root, HomePage.ModelTypeAlias);
        var homePublishResult = ContentService.Publish(homeNode, []);

        homePublishResult.Success.Should().BeTrue();
        
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

        var updateResult = await DomainService.UpdateDomainsAsync(homeNode.Key, new DomainsUpdateModel
        {
            Domains = [englishDomain, danishDomain],
        });
        
        updateResult.Success.Should().BeTrue();

        var assigned = await DomainService.GetAssignedDomainsAsync(homeNode.Key, true);
        
        assigned.Should().HaveCount(2);

        var publishedAssigned = UmbracoContext.Domains.GetAssigned(homeNode.Id, true);

        publishedAssigned.Should().HaveCount(2);

        // Create the other content
        var contentNode = ContentService.CreateAndSave("Content", homeNode, ContentPage.ModelTypeAlias);
        var contentPublishResult = ContentService.Publish(contentNode, []);

        contentPublishResult.Success.Should().BeTrue();

        var hiddenNode = ContentService.CreateAndSave("Hidden", contentNode, HiddenNode.ModelTypeAlias);
        var hiddenPublishResult = ContentService.Publish(hiddenNode, []);

        hiddenPublishResult.Success.Should().BeTrue();

        var nestedNode = ContentService.CreateAndSave("Nested", hiddenNode, ContentPage.ModelTypeAlias);
        var nestedPublishResult = ContentService.Publish(nestedNode, []);

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