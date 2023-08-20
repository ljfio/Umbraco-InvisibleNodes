// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Our.UmbracoCms.VirtualNodes.Core;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Our.UmbracoCms.VirtualNodes
{
    public class VirtualNodeUrlProvider : IUrlProvider
    {
        private readonly ISiteDomainHelper _siteDomainHelper;
        private readonly IVirtualNodeRulesManager _rulesManager;

        public VirtualNodeUrlProvider(
            ISiteDomainHelper siteDomainHelper,
            IVirtualNodeRulesManager rulesManager)
        {
            _siteDomainHelper = siteDomainHelper;
            _rulesManager = rulesManager;
        }

        /// <inheritdoc />
        public UrlInfo GetUrl(
            UmbracoContext umbracoContext, 
            IPublishedContent content, 
            UrlMode mode,
            string culture,
            Uri current)
        {
            string route = GenerateRoute(content, culture);

            if (mode == UrlMode.Auto || mode == UrlMode.Default || mode == UrlMode.Relative)
                return UrlInfo.Url(route, culture);

            if (umbracoContext == null || umbracoContext.Domains == null)
                return null;

            var domainCache = umbracoContext.Domains;

            var domainAndUris = domainCache.GetAll(false)
                .Select(domain => new DomainAndUri(domain, current))
                .ToList();

            var mappedDomain = _siteDomainHelper.MapDomain(domainAndUris, current, culture, domainCache.DefaultCulture);

            if (mappedDomain == null)
                return UrlInfo.Url(route, culture);

            var builder = new UriBuilder(mappedDomain.Uri)
            {
                Path = DomainUtilities.PathRelativeToDomain(mappedDomain.Uri, route),
            };

            return UrlInfo.Url(builder.ToString(), culture);
        }

        /// <inheritdoc />
        public IEnumerable<UrlInfo> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            if (umbracoContext == null ||
                umbracoContext.Content == null ||
                umbracoContext.Domains == null)
                return Enumerable.Empty<UrlInfo>();

            var content = umbracoContext.Content.GetById(id);

            if (content == null)
                return Enumerable.Empty<UrlInfo>();

            string route = GenerateRoute(content);

            var domainCache = umbracoContext.Domains;

            var domainAndUris = domainCache.GetAll(false)
                .Select(domain => new DomainAndUri(domain, current))
                .ToList();

            var mappedDomains =
                _siteDomainHelper.MapDomains(domainAndUris, current, true, null, domainCache.DefaultCulture);

            var urls = new List<UrlInfo>();

            foreach (var mappedDomain in mappedDomains)
            {
                var builder = new UriBuilder(mappedDomain.Uri)
                {
                    Path = DomainUtilities.PathRelativeToDomain(mappedDomain.Uri, route)
                };

                urls.Add(UrlInfo.Url(builder.ToString()));
            }

            return urls;
        }

        /// <summary>
        /// Generates out the correct route based on the 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        private string GenerateRoute(IPublishedContent content, string culture = null)
        {
            var segments = content.AncestorsOrSelf()
                .Where(ancestor => !ancestor.IsVirtualNode(_rulesManager) && ancestor.Level > 1)
                .Select(ancestor => ancestor.UrlSegment(culture))
                .Reverse()
                .ToList();

            return string.Join("/", segments)
                .EnsureStartsWith('/')
                .EnsureEndsWith('/');
        }
    }
}