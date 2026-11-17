// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.InvisibleNodes.Routing;

/// <summary>
/// Replaces the Umbraco content URL provider so that it no longer generates content URLs
/// </summary>
/// <remarks>
/// Umbraco combines the other URLs of every URL provider, so the built-in provider would list the URLs that include
/// the segments of invisible nodes alongside those from the <see cref="InvisibleNodeUrlProvider"/>. The built-in
/// provider is kept for everything else Umbraco uses it for, such as preview URLs and redirect URL management.
/// </remarks>
public class InvisibleNodeDefaultUrlProvider : DefaultUrlProvider
{
    public InvisibleNodeDefaultUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        ILogger<DefaultUrlProvider> logger,
        ISiteDomainMapper siteDomainMapper,
        IUmbracoContextAccessor umbracoContextAccessor,
        UriUtility uriUtility,
        IPublishedContentCache publishedContentCache,
        IDomainCache domainCache,
        IIdKeyMap idKeyMap,
        IDocumentUrlService documentUrlService,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
        ILanguageService languageService)
        : base(
            requestSettings,
            logger,
            siteDomainMapper,
            umbracoContextAccessor,
            uriUtility,
            publishedContentCache,
            domainCache,
            idKeyMap,
            documentUrlService,
            navigationQueryService,
            publishedContentStatusFilteringService,
            languageService)
    {
    }

    /// <inheritdoc />
    public override UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current) => null;

    /// <inheritdoc />
    public override IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current) => Enumerable.Empty<UrlInfo>();
}
