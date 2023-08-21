// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Our.Umbraco.InvisibleNodes;

public class InvisibleNodeSettings
{
    public const string InvisibleNodes = "InvisibleNodes";
    
    public string[] ContentTypes { get; set; } = Array.Empty<string>();
}