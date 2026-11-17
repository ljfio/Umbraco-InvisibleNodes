// Copyright 2023-2025 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

namespace Our.Umbraco.InvisibleNodes.Core;

/// <summary>
/// The behavior for a request to a URL that includes the segment of an invisible node,
/// i.e. the URL the node would have without this package
/// </summary>
public enum NativeUrlBehavior
{
    /// <summary>
    /// Permanently redirect to the URL without the invisible segments,
    /// requests for the invisible node itself are not found
    /// </summary>
    Redirect = 0,

    /// <summary>
    /// The request is not found
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// Umbraco serves the content as it would without this package
    /// </summary>
    Allow = 2,
}
