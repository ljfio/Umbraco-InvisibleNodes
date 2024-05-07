// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Our.Umbraco.InvisibleNodes.Caching;
using Our.Umbraco.InvisibleNodes.Core;
using Our.Umbraco.InvisibleNodes.Core.Caching;
using Our.Umbraco.InvisibleNodes.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Our.Umbraco.InvisibleNodes.Composing;

public class PackageComposer : IComposer
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
            .AddSingleton<IInvisibleNodeLocator, InvisibleNodeLocator>()
            .AddSingleton<IInvisibleNodeRulesManager, InvisibleNodeRulesManager>();

        builder.Services
            .AddSingleton<IInvisibleNodeCache>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<InvisibleNodeSettings>>();
               
                if (settings.Value.CachingEnabled)
                    return new InvisibleNodeCache(provider.GetRequiredService<AppCaches>());

                return new NoOpNodeCache();
            });

        builder
            .AddNotificationHandler<ContentSavingNotification, InvalidateCacheNotificationHandler>()
            .AddNotificationHandler<ContentMovingNotification, InvalidateCacheNotificationHandler>()
            .AddNotificationHandler<ContentMovingToRecycleBinNotification, InvalidateCacheNotificationHandler>();
    }
}