using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.VirtualNodes.Core;

public interface IVirtualNodeRulesManager
{
    bool IsVirtualNode(IPublishedContent content);
}