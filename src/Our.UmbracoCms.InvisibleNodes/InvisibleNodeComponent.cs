// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;

namespace Our.UmbracoCms.InvisibleNodes
{
    public class InvisibleNodeComponent : IComponent
    {
        private readonly IInvisibleNodeCache _invisibleNodeCache;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public InvisibleNodeComponent(
            IInvisibleNodeCache invisibleNodeCache,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            _invisibleNodeCache = invisibleNodeCache;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public void Initialize()
        {
            ContentService.Published += ContentServiceOnPublished;
        }

        public void Terminate()
        {
            ContentService.Published -= ContentServiceOnPublished;
        }

        private void ContentServiceOnPublished(IContentService sender, ContentPublishedEventArgs e)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;

            foreach (var publishedEntity in e.PublishedEntities.EmptyNull())
            {
                foreach (var culture in publishedEntity.PublishedCultures)
                {
                    string url = umbracoContext.UrlProvider.GetUrl(publishedEntity.Id, UrlMode.Absolute, culture);

                    var uri = new Uri(url);

                    _invisibleNodeCache.ClearRoute(uri.Host, uri.AbsolutePath);
                }
            }
        }
    }
}