// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.VirtualNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Our.Umbraco.VirtualNodes;

public static class VirtualNodeContentExtensions
{
    public static bool IsVirtualNode(this IPublishedContent content, IVirtualNodeRulesManager rulesManager)
    {
        return rulesManager.IsVirtualNode(content);
    }

    public static bool IsVirtualNode(this IPublishedContent content)
    {
        var rulesManager = StaticServiceProvider.Instance.GetRequiredService<IVirtualNodeRulesManager>();
        return IsVirtualNode(content, rulesManager);
    }
}