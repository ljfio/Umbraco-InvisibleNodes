// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.InvisibleNodes.Core;

public interface IInvisibleNodeRulesManager
{
    bool IsInvisibleNode(IPublishedContent content);
}