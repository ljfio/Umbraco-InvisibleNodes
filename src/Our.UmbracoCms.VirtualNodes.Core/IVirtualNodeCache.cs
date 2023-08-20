// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

namespace Our.UmbracoCms.VirtualNodes.Core
{
    public interface IVirtualNodeCache
    {
        int? GetRoute(string host, string path);
        void StoreRoute(string host, string path, int id);
        void ClearAll();
        void ClearHost(string host);
        void ClearRoute(string host, string path);
    }
}