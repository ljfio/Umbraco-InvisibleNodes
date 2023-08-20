// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Our.Umbraco.VirtualNodes;

public class VirtualNodeSettings
{
    public const string VirtualNode = "VirtualNode";
    
    public string[] ContentTypes { get; set; } = Array.Empty<string>();
}