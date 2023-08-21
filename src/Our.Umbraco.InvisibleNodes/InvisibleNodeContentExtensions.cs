// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Our.Umbraco.InvisibleNodes;

public static class InvisibleNodeContentExtensions
{
    public static bool IsInvisibleNode(this IPublishedContent content, IInvisibleNodeRulesManager rulesManager)
    {
        return rulesManager.IsInvisibleNode(content);
    }

    public static bool IsInvisibleNode(this IPublishedContent content)
    {
        var rulesManager = StaticServiceProvider.Instance.GetRequiredService<IInvisibleNodeRulesManager>();
        return IsInvisibleNode(content, rulesManager);
    }
}