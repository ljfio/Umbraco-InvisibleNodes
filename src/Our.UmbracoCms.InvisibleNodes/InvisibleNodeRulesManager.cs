// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Configuration;
using System.Linq;
using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.InvisibleNodes
{
    public class InvisibleNodeRulesManager : IInvisibleNodeRulesManager
    {
        private readonly string[] _contentTypes;

        public InvisibleNodeRulesManager()
        {
            string value = ConfigurationManager.AppSettings.Get("VirtualNode") ?? string.Empty;

            _contentTypes = value
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();
        }

        public bool IsInvisibleNode(IPublishedContent content)
        {
            return _contentTypes.Contains(content.ContentType.Alias);
        }
    }
}