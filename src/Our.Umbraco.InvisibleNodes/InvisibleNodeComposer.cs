// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Our.Umbraco.InvisibleNodes;

public class InvisibleNodeComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ContentFinders()
            .Insert<InvisibleNodeContentFinder>();

        builder.UrlProviders()
            .Insert<InvisibleNodeUrlProvider>();

        builder.Services
            .Configure<InvisibleNodeSettings>(builder.Config.GetSection(InvisibleNodeSettings.InvisibleNodes));
        
        builder.Services
            .AddSingleton<IInvisibleNodeCache, InvisibleNodeCache>()
            .AddSingleton<IInvisibleNodeRulesManager, InvisibleNodeRulesManager>();
    }
}