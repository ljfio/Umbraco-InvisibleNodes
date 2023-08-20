// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.VirtualNodes.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Our.Umbraco.VirtualNodes;

public class VirtualNodeComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ContentFinders()
            .Insert<VirtualNodeContentFinder>();

        builder.UrlProviders()
            .Insert<VirtualNodeUrlProvider>();

        builder.Services
            .Configure<VirtualNodeSettings>(builder.Config.GetSection(VirtualNodeSettings.VirtualNode));

        builder.Services
            .AddSingleton<IVirtualNodeCache, VirtualNodeCache>()
            .AddSingleton<IVirtualNodeRulesManager, VirtualNodeRulesManager>();
    }
}