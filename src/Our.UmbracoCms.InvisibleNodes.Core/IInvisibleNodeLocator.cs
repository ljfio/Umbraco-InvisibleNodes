// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.InvisibleNodes.Core
{
    public interface IInvisibleNodeLocator
    {
        /// <summary>
        /// Walks the published content tree to locate a node that may be virtually hidden
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        IPublishedContent Locate(IPublishedContent node, string path, string culture);
    }
}