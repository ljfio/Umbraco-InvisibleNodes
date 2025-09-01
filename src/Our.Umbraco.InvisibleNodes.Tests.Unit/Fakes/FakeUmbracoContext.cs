// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakeUmbracoContext : IUmbracoContext
{
    public FakeUmbracoContext(
        FakePublishedContentCache content,
        FakePublishedMediaCache media,
        FakeDomainCache domains)
    {
        Content = content;
        Media = media;
        Domains = domains;
    }

    void IDisposable.Dispose()
    {
    }

    public DateTime ObjectCreated { get; } = DateTime.UtcNow;

    public Uri OriginalRequestUrl { get; set; } = null!;

    public Uri CleanedUmbracoUrl { get; set; } = null!;

    public IPublishedContentCache Content { get; }

    public IPublishedMediaCache Media { get; }

    public IDomainCache Domains { get; }

    public IPublishedRequest? PublishedRequest { get; set; }

    public bool IsDebug { get; set; }

    public bool InPreviewMode { get; set; }
}

public static class FakeUmbracoContextExtensions
{
    /// <summary>
    /// Creates an <see cref="IUmbracoContextAccessor"/> for the <see cref="FakeUmbracoContext"/>
    /// </summary>
    /// <param name="umbracoContext"></param>
    /// <returns></returns>
    public static IUmbracoContextAccessor GetUmbracoContextAccessor(this FakeUmbracoContext umbracoContext)
    {
        var accessor = new FakeUmbracoContextAccessor();
        accessor.Set(umbracoContext);
        return accessor;
    }
}