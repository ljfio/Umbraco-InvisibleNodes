// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakeDocumentUrlService : IDocumentUrlService
{
    private readonly FakePublishedCache _publishedCache;

    public FakeDocumentUrlService(FakePublishedCache publishedCache)
    {
        _publishedCache = publishedCache;
    }

    public Task InitAsync(bool forceEmpty, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RebuildAllUrlsAsync()
    {
        throw new NotImplementedException();
    }

    public string? GetUrlSegment(Guid documentKey, string culture, bool isDraft)
    {
        var publishedContent = _publishedCache.GetById(isDraft, documentKey);

        if (publishedContent is null)
            return null;

        return publishedContent.Cultures
            .TryGetValue(culture, out var i) ? i.UrlSegment : null;
    }

    public Task CreateOrUpdateUrlSegmentsAsync(Guid key)
    {
        throw new NotImplementedException();
    }

    public Task CreateOrUpdateUrlSegmentsWithDescendantsAsync(Guid key)
    {
        throw new NotImplementedException();
    }

    public Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documents)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUrlsFromCacheAsync(IEnumerable<Guid> documentKeys)
    {
        throw new NotImplementedException();
    }

    public Guid? GetDocumentKeyByRoute(string route, string? culture, int? documentStartNodeId, bool isDraft)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UrlInfo>> ListUrlsAsync(Guid contentKey)
    {
        throw new NotImplementedException();
    }

    public string GetLegacyRouteFormat(Guid key, string? culture, bool isDraft)
    {
        throw new NotImplementedException();
    }

    public bool HasAny()
    {
        throw new NotImplementedException();
    }
}