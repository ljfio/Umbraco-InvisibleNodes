// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.InvisibleNodes.Core
{
    public interface IInvisibleNodeRulesManager
    {
        bool IsInvisibleNode(IPublishedContent content);
    }
}