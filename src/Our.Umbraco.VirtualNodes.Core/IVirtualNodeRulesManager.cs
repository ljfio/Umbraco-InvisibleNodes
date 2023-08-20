// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.VirtualNodes.Core;

public interface IVirtualNodeRulesManager
{
    bool IsVirtualNode(IPublishedContent content);
}