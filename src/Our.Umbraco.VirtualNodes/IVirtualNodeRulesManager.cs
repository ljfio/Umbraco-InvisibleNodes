using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.VirtualNodes;

public interface IVirtualNodeRulesManager
{
    bool IsVirtualNode(IPublishedContent content);
}