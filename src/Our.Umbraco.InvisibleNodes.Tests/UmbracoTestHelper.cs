// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.InvisibleNodes.Tests;

public static class UmbracoTestHelper
{
    public static IEnumerable<Domain> GenerateDomains(params string[] urls)
    {
        int current = 1;

        foreach (var url in urls)
        {
            yield return GenerateDomain(url, current++);
        }
    }

    public static Domain GenerateDomain(string url, int id)
    {
#if NET7_0_OR_GREATER
        return new Domain(id, url, id, null, false, id);
#else
        return new Domain(id, url, id, null, false);
#endif
    }

    public static IUmbracoContextAccessor GenerateUmbracoContextAccessor(
        IEnumerable<Domain>? domains = null,
        IEnumerable<IPublishedContent>? content = null)
    {
        var mockUmbracoContextAccessor = new Mock<IUmbracoContextAccessor>();

        var umbracoContext = GenerateUmbracoContext(domains, content);

        mockUmbracoContextAccessor
            .Setup(m => m.TryGetUmbracoContext(out umbracoContext))
            .Returns(true);

        return mockUmbracoContextAccessor.Object;
    }

    public static IUmbracoContext GenerateUmbracoContext(
        IEnumerable<Domain>? domains = null,
        IEnumerable<IPublishedContent>? content = null)
    {
        var mockDomainCache = new Mock<IDomainCache>();

        if (domains is not null)
        {
            mockDomainCache
                .Setup(m => m.GetAll(It.IsAny<bool>()))
                .Returns(domains);

            mockDomainCache
                .Setup(m => m.GetAssigned(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(domains);
        }

        var mockPublishedContentCache = new Mock<IPublishedContentCache>();

        if (content is not null)
        {
            mockPublishedContentCache
                .Setup(m => m.GetById(It.IsAny<int>()))
                .Returns((int id) => content.FirstOrDefault(c => c.Id == id));
        }

        var mockUmbracoContext = new Mock<IUmbracoContext>();

        mockUmbracoContext
            .Setup(m => m.Domains)
            .Returns(mockDomainCache.Object);

        mockUmbracoContext
            .Setup(m => m.Content)
            .Returns(mockPublishedContentCache.Object);

        return mockUmbracoContext.Object;
    }

    public static IPublishedContent GenerateNode(
        int id,
        string name,
        string segment,
        IPublishedContent? parent = null,
        string? culture = null)
    {
        var mock = new Mock<IPublishedContent>();
        var mockType = new Mock<IPublishedContentType>();

        mockType.Setup(m => m.Variations)
            .Returns(string.IsNullOrEmpty(culture) ? ContentVariation.Nothing : ContentVariation.Culture);

        mock.Setup(m => m.ContentType)
            .Returns(mockType.Object);

        string key = culture ?? string.Empty;

        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [key] = new(key, name, segment, DateTime.Now)
        };

        mock.Setup(m => m.Id)
            .Returns(id);

        mock.Setup(m => m.Level)
            .Returns(parent?.Level + 1 ?? 1);

        mock.Setup(m => m.Cultures)
            .Returns(cultures);

        mock.Setup(m => m.Parent)
            .Returns(parent);

        return mock.Object;
    }
}