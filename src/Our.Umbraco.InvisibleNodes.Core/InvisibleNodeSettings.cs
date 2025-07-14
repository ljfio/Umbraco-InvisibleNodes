// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics.CodeAnalysis;

namespace Our.Umbraco.InvisibleNodes.Core;

[ExcludeFromCodeCoverage]
public class InvisibleNodeSettings
{
    public const string InvisibleNodes = "InvisibleNodes";

    /// <summary>
    /// Toggles if nodes are caches when located
    /// </summary>
    public bool CachingEnabled { get; set; } = true;
    
    /// <summary>
    /// Defines the content types for the nodes that are invisible
    /// </summary>
    public string[] ContentTypes { get; set; } = Array.Empty<string>();
}