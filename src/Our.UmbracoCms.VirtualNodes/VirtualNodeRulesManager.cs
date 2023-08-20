// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using System.Configuration;
using System.Linq;
using Our.UmbracoCms.VirtualNodes.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Our.UmbracoCms.VirtualNodes
{
    public class VirtualNodeRulesManager : IVirtualNodeRulesManager
    {
        private readonly string[] _contentTypes;

        public VirtualNodeRulesManager()
        {
            string value = ConfigurationManager.AppSettings.Get("VirtualNode") ?? string.Empty;

            _contentTypes = value
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();
        }

        public bool IsVirtualNode(IPublishedContent content)
        {
            return _contentTypes.Contains(content.ContentType.Alias);
        }
    }
}