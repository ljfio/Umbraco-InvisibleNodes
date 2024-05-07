// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Our.Umbraco.InvisibleNodes.Core.Caching;

namespace Our.Umbraco.InvisibleNodes.Caching;

/// <summary>
/// Implements a non-operating version of the <see cref="IInvisibleNodeCache"/>
/// </summary>
public class NoOpNodeCache : IInvisibleNodeCache
{
    public int? GetRoute(string host, string path)
    {
        return null;
    }

    public void StoreRoute(string host, string path, int id)
    {
        // Intentionally left blank
    }

    public void ClearAll()
    {
        // Intentionally left blank
    }

    public void ClearHost(string host)
    {
        // Intentionally left blank
    }

    public void ClearRoute(string host, string path)
    {
        // Intentionally left blank
    }
}