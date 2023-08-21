// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using Microsoft.Extensions.Options;
using Our.Umbraco.InvisibleNodes.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.InvisibleNodes;

public class InvisibleNodeRulesManager : IInvisibleNodeRulesManager
{
    private InvisibleNodeSettings _settings;
    
    public InvisibleNodeRulesManager(
        IOptionsMonitor<InvisibleNodeSettings> optionsMonitor)
    {
        _settings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(settings => _settings = settings);
    }

    public bool IsInvisibleNode(IPublishedContent content)
    {
        return _settings.ContentTypes.Contains(content.ContentType.Alias);
    }
}