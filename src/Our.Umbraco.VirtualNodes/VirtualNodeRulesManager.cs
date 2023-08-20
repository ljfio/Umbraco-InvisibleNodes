// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Microsoft.Extensions.Options;
using Our.Umbraco.VirtualNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.VirtualNodes;

public class VirtualNodeRulesManager : IVirtualNodeRulesManager
{
    private VirtualNodeSettings _settings;
    
    public VirtualNodeRulesManager(
        IOptionsMonitor<VirtualNodeSettings> optionsMonitor)
    {
        _settings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(settings => _settings = settings);
    }

    public bool IsVirtualNode(IPublishedContent content)
    {
        return _settings.ContentTypes.Contains(content.ContentType.Alias);
    }
}