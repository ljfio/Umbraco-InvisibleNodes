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

public abstract class IntegrationTestBase(TestWebApplicationFactory factory) : IDisposable
{
    private IContentService? _contentService;
    private IDomainService? _domainService;
    private ILocalizationService? _localizationService;

    protected IServiceProvider Services => factory.Services;

    protected HttpClient HttpClient => factory.CreateClient();

    protected IContentService ContentService =>
        _contentService ??= Services.GetRequiredService<IContentService>();

    protected IDomainService DomainService =>
        _domainService ??= Services.GetRequiredService<IDomainService>();

    protected ILocalizationService LocalizationService =>
        _localizationService ??= Services.GetRequiredService<ILocalizationService>();

    protected IPublishedUrlProvider PublishedUrlProvider => Services.GetRequiredService<IPublishedUrlProvider>();

    private UmbracoContextReference UmbracoContextReference => Services
        .GetRequiredService<IUmbracoContextFactory>()
        .EnsureUmbracoContext();

    protected IUmbracoContext UmbracoContext => UmbracoContextReference.UmbracoContext;

    public void Dispose()
    {
        // Cleanup content if used
        if (_contentService is not null)
            foreach (var content in _contentService.GetRootContent())
                _contentService.Delete(content);

        // Cleanup domains if used
        if (_domainService is not null)
            foreach (var content in _domainService.GetAll(true))
                _domainService.Delete(content);
    }
}