// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakePublishedContentType : IPublishedContentType
{
    public int GetPropertyIndex(string alias)
    {
        throw new NotImplementedException();
    }

    public IPublishedPropertyType? GetPropertyType(string alias)
    {
        throw new NotImplementedException();
    }

    public IPublishedPropertyType? GetPropertyType(int index)
    {
        throw new NotImplementedException();
    }

    public Guid Key { get; set; }
    public int Id { get; set; }
    public string Alias { get; set; }
    public PublishedItemType ItemType { get; set; }
    public HashSet<string> CompositionAliases { get; set; }
    public ContentVariation Variations { get; set; }
    public bool IsElement { get; set; }
    public IEnumerable<IPublishedPropertyType> PropertyTypes { get; set; }
}