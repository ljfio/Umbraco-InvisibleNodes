// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Our.UmbracoCms.VirtualNodes.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.VirtualNodes
{
    public static class VirtualNodeContentExtensions
    {
        public static bool IsVirtualNode(this IPublishedContent content, IVirtualNodeRulesManager rulesManager)
        {
            return rulesManager.IsVirtualNode(content);
        }
    }
}