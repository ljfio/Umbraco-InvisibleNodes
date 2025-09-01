using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public static class FakePublishedCacheExtensions
{
    private static readonly IShortStringHelper ShortStringHelper =
        new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="segment"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="children"></param>
    /// <param name="culture"></param>
    /// <param name="preview"></param>
    /// <typeparam name="TCache"></typeparam>
    /// <returns></returns>
    public static IPublishedContent Generate<TCache>(
        this TCache cache,
        string name,
        string segment,
        IPublishedContent? parent = null,
        IEnumerable<IPublishedContent>? children = null,
        string? culture = null,
        bool preview = false) where TCache : FakePublishedCache
    {
        // var contentType = cache.GetContentType(contentTypeAlias);
        //
        // if (contentType is null)
        //     return null;
        
        var content = new FakePublishedContent
        {
            Name = name,
            UrlSegment = ShortStringHelper.CleanStringForUrlSegment(name),
        };

        cache.Add(content);

        return content;
    }
}