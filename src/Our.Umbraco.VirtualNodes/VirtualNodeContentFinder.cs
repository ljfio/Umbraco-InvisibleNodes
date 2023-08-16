using System;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.VirtualNodes;

public class VirtualNodeContentFinder : IContentFinder
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public VirtualNodeContentFinder(
        IUmbracoContextAccessor umbracoContextAccessor, 
        IVariationContextAccessor variationContextAccessor)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _variationContextAccessor = variationContextAccessor;
    }

    /// <inheritdoc />
    public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var context) || context.Content is null)
            return false;

        string route = request.Uri.AbsolutePath.Trim('/');
        string? culture = request.Culture;

        var root = request.Domain is not null
            ? context.Content.GetById(request.Domain.ContentId)
            : context.Content.GetAtRoot(culture).FirstOrDefault();

        string[] segments = route.Split('/');

        if (root is null || segments.Length == 0)
        {
            request.SetPublishedContent(root);
            return true;
        }

        var foundNode = WalkContentTree(root, segments, culture);

        if (foundNode is not null)
        {
            request.SetPublishedContent(foundNode);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Walks the published content tree to locate a that may be virtually hidden
    /// </summary>
    /// <param name="node"></param>
    /// <param name="segments"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    private IPublishedContent? WalkContentTree(IPublishedContent node, string[] segments, string? culture)
    {
        string segment = segments.First();

        if (segments.Length == 0 && string.Equals(node.UrlSegment(_variationContextAccessor, culture), segment))
        {
            return node;
        }
        
        string[] childSegments = segments.Skip(1).ToArray();

        foreach (var child in node.Children.EmptyNull())
        {
            if (string.Equals(child.UrlSegment(_variationContextAccessor, culture), segment))
            {
                if (segments.Length == 1)
                    return child;

                var grandChild = WalkContentTree(child, childSegments, culture);

                if (grandChild != null)
                    return grandChild;
            }

            if (child.IsVirtualNode())
            {
                var hiddenChild = WalkContentTree(child, segments, culture);

                if (hiddenChild is not null)
                    return hiddenChild;
            }
        }

        return null;
    }
}