// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
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

        private readonly IDictionary<int, IList<string>> _previousUrls = new Dictionary<int, IList<string>>();

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
            ContentService.Moving += ContentServiceOnMoving;
            ContentService.Moved += ContentServiceOnMoved;
        }

        public void Terminate()
        {
            ContentService.Moved -= ContentServiceOnMoved;
            ContentService.Moving -= ContentServiceOnMoving;
            ContentService.Published -= ContentServiceOnPublished;
        }

        #region Published

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

        #endregion

        #region Moving / Moved

        private void ContentServiceOnMoving(IContentService sender, MoveEventArgs<IContent> e)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;

            foreach (var moveEventInfo in e.MoveInfoCollection)
            {
                var entity = moveEventInfo.Entity;

                var urls = new List<string>();

                foreach (var culture in entity.PublishedCultures)
                {
                    urls.Add(umbracoContext.UrlProvider.GetUrl(entity.Id, UrlMode.Absolute, culture));
                }

                _previousUrls[entity.Id] = urls;
            }
        }

        private void ContentServiceOnMoved(IContentService sender, MoveEventArgs<IContent> e)
        {
            foreach (var moveEventInfo in e.MoveInfoCollection)
            {
                var entity = moveEventInfo.Entity;

                if (_previousUrls.TryGetValue(entity.Id, out var urls))
                {
                    foreach (var url in urls)
                    {
                        var uri = new Uri(url);
                        _invisibleNodeCache.ClearRoute(uri.Host, uri.AbsolutePath);
                    }

                    _previousUrls.Remove(entity.Id);
                }
            }
        }

        #endregion
    }
}