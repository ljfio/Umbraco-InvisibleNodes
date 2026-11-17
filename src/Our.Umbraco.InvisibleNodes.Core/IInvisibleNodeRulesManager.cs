// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.InvisibleNodes.Core;

public interface IInvisibleNodeRulesManager
{
    bool IsInvisibleNode(IPublishedContent content);

    /// <summary>
    /// The aliases of the content types that are invisible, when the rules depend only on the content type
    /// </summary>
    /// <remarks>
    /// When set, the invisible nodes are found from the navigation structure without loading each node's content,
    /// which is much faster for nodes with many children. Return <c>null</c> (the default) when
    /// <see cref="IsInvisibleNode"/> depends on more than the content type alias.
    /// </remarks>
    IReadOnlyCollection<string>? InvisibleContentTypes => null;
}
