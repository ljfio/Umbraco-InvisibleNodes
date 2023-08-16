using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.VirtualNodes;

public static class VirtualNodeContentExtensions
{
    public static bool IsVirtualNode(this IPublishedContent content)
    {
        return false;
    }
}