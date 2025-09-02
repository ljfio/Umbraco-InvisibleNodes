// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Services.Navigation;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakeDocumentNavigationService : IDocumentNavigationQueryService, IDocumentNavigationManagementService
{
    private readonly ConcurrentDictionary<Guid, NavigationNode> _structure = new();
    private readonly ConcurrentDictionary<Guid, NavigationNode> _recycleBinStructure = new();

    private readonly ConcurrentBag<Guid> _rootKeys = new();
    private readonly ConcurrentBag<Guid> _recycleBinRootKeys = new();

    private readonly FakePublishedCache _cache;

    public FakeDocumentNavigationService(FakePublishedCache cache)
    {
        _cache = cache;
    }

    public bool TryGetParentKey(Guid childKey, out Guid? parentKey)
    {
        if (_structure.TryGetValue(childKey, out var node))
        {
            parentKey = node.Parent;
            return true;
        }

        parentKey = null;
        return false;
    }

    public bool TryGetRootKeys(out IEnumerable<Guid> rootKeys)
    {
        rootKeys = _rootKeys;
        return true;
    }

    public bool TryGetRootKeysOfType(string contentTypeAlias, out IEnumerable<Guid> rootKeys)
    {
        var contentType = _cache.GetContentType(contentTypeAlias);

        var matchingKeys = new List<Guid>();

        if (contentType is null)
        {
            rootKeys = matchingKeys;
            return false;
        }

        foreach (var rootKey in _rootKeys)
            if (_structure.TryGetValue(rootKey, out var node) && node.ContentTypeKey == contentType.Key)
                matchingKeys.Add(rootKey);

        rootKeys = matchingKeys;
        return matchingKeys.Count > 0;
    }

    public bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys)
    {
        if (_structure.TryGetValue(parentKey, out var node))
        {
            childrenKeys = node.Children;
            return true;
        }

        childrenKeys = [];
        return false;
    }

    public bool TryGetChildrenKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> childrenKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetDescendantsKeysOfType(Guid parentKey, string contentTypeAlias,
        out IEnumerable<Guid> descendantsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
    {
        var keys = new List<Guid>();
        Guid key = childKey;

        while (_structure.TryGetValue(key, out var node) && node.Parent.HasValue)
        {
            key = node.Parent.Value;
            keys.Add(key);
        }

        ancestorsKeys = keys;
        return keys.Count > 0;
    }

    public bool TryGetAncestorsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> ancestorsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetSiblingsKeysOfType(Guid key, string contentTypeAlias, out IEnumerable<Guid> siblingsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLevel(Guid contentKey, [NotNullWhen(true)] out int? level)
    {
        throw new NotImplementedException();
    }

    public bool TryGetParentKeyInBin(Guid childKey, out Guid? parentKey)
    {
        throw new NotImplementedException();
    }

    public bool TryGetChildrenKeysInBin(Guid parentKey, out IEnumerable<Guid> childrenKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetDescendantsKeysInBin(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetAncestorsKeysInBin(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
    {
        throw new NotImplementedException();
    }

    public bool TryGetSiblingsKeysInBin(Guid key, out IEnumerable<Guid> siblingsKeys)
    {
        throw new NotImplementedException();
    }

    public Task RebuildAsync()
    {
        throw new NotImplementedException();
    }

    public bool MoveToBin(Guid key)
    {
        throw new NotImplementedException();
    }

    public bool Add(Guid key, Guid contentTypeKey, Guid? parentKey = null, int? sortOrder = null)
    {
        var node = new NavigationNode(key, contentTypeKey, sortOrder ?? 0);
        
        _structure.TryAdd(key, node);

        if (parentKey.HasValue && _structure.TryGetValue(parentKey.Value, out var parent))
        {
            parent.AddChild(_structure, key);
            return true;
        }

        _rootKeys.Add(key);
        return true;
    }

    public bool Move(Guid key, Guid? targetParentKey = null)
    {
        throw new NotImplementedException();
    }

    public bool UpdateSortOrder(Guid key, int newSortOrder)
    {
        if (_structure.TryGetValue(key, out var node))
        {
            node.UpdateSortOrder(newSortOrder);
            return true;
        }

        return false;
    }

    public Task RebuildBinAsync()
    {
        throw new NotImplementedException();
    }

    public bool RemoveFromBin(Guid key)
    {
        throw new NotImplementedException();
    }

    public bool RestoreFromBin(Guid key, Guid? targetParentKey = null)
    {
        throw new NotImplementedException();
    }
}