// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.InvisibleNodes
{
    public static class InvisibleNodeContentExtensions
    {
        public static bool IsInvisibleNode(this IPublishedContent content, IInvisibleNodeRulesManager rulesManager)
        {
            return rulesManager.IsInvisibleNode(content);
        }
    }
}