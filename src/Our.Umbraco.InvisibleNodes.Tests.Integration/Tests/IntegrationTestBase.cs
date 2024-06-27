// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.InvisibleNodes.Tests.Integration.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.InvisibleNodes.Tests.Integration.Tests;

public abstract class IntegrationTestBase(TestWebApplicationFactory factory)
{
    protected IServiceProvider Services => factory.Services;

    protected HttpClient HttpClient => factory.CreateClient();

    protected IContentService ContentService => Services.GetRequiredService<IContentService>();

    protected IDomainService DomainService => Services.GetRequiredService<IDomainService>();

    protected ILocalizationService LocalizationService => Services.GetRequiredService<ILocalizationService>();

    protected IPublishedUrlProvider PublishedUrlProvider => Services.GetRequiredService<IPublishedUrlProvider>();

    private UmbracoContextReference UmbracoContextReference => Services
        .GetRequiredService<IUmbracoContextFactory>()
        .EnsureUmbracoContext();

    protected IUmbracoContext UmbracoContext => UmbracoContextReference.UmbracoContext;
}