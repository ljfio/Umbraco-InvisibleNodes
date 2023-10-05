// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core.Cache;

namespace Our.UmbracoCms.InvisibleNodes
{
    public class InvisibleNodeCache : IInvisibleNodeCache
    {
        private readonly IAppPolicyCache _cache;

        public InvisibleNodeCache(AppCaches appCaches)
        {
            _cache = appCaches.IsolatedCaches.GetOrCreate<InvisibleNodeCache>();
        }

        public int? GetRoute(string host, string path)
        {
            return _cache.GetCacheItem<int?>(GenerateKey(host, path));
        }

        public void StoreRoute(string host, string path, int id)
        {
            _cache.InsertCacheItem(GenerateKey(host, path), () => id);
        }

        public void ClearAll()
        {
            _cache.ClearByKey(GenerateKey());
        }

        public void ClearHost(string host)
        {
            _cache.ClearByKey(GenerateKey(host));
        }

        public void ClearRoute(string host, string path)
        {
            _cache.ClearByKey(GenerateKey(host, path));
        }

        private string GenerateKey(string host = null, string path = null)
        {
            var builder = new StringBuilder("Route::");

            if (!string.IsNullOrEmpty(host))
                builder
                    .Append(host);

            if (!string.IsNullOrEmpty(path))
                builder
                    .Append("::")
                    .Append(path);

            return builder.ToString();
        }
    }
}