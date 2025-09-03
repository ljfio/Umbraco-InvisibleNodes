// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakePublishedStatusFilteringService : IPublishedContentStatusFilteringService
{
    private readonly IPublishedCache _cache;

    public FakePublishedStatusFilteringService(IPublishedCache cache)
    {
        _cache = cache;
    }

    public IEnumerable<IPublishedContent> FilterAvailable(IEnumerable<Guid> candidateKeys, string? culture)
    {
        return candidateKeys
            .Select(_cache.GetById)
            .WhereNotNull()
            .ToList();
    }
}