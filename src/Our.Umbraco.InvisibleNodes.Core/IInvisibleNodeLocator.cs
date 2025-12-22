// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Our.Umbraco.InvisibleNodes.Core;

public interface IInvisibleNodeLocator
{
    /// <summary>
    /// Walks the published content tree to locate a node that may be virtually hidden
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="node"></param>
    /// <param name="path"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    Task<IPublishedContent?> Locate(IPublishedContentCache cache, IPublishedContent node, string path, string culture);
}