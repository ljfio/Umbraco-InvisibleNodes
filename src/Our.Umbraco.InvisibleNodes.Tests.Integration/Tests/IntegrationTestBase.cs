// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
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

    protected HttpClient NonRedirectingHttpClient => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    protected IContentService ContentService => Services.GetRequiredService<IContentService>();

    protected IDomainService DomainService => Services.GetRequiredService<IDomainService>();
    
    protected IPublishedUrlProvider PublishedUrlProvider => Services.GetRequiredService<IPublishedUrlProvider>();

    private UmbracoContextReference UmbracoContextReference => Services
        .GetRequiredService<IUmbracoContextFactory>()
        .EnsureUmbracoContext();

    protected IUmbracoContext UmbracoContext => UmbracoContextReference.UmbracoContext;

    /// <summary>
    /// Changes a setting of the running site, as if appsettings.json had been edited
    /// </summary>
    protected void SetConfiguration(string key, string? value)
    {
        var configuration = (IConfigurationRoot)Services.GetRequiredService<IConfiguration>();

        configuration[key] = value;
        configuration.Reload();
    }
}
