using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Strings;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public static class FakePublishedCacheExtensions
{
    private static readonly IShortStringHelper ShortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    private static int _id;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="name"></param>
    /// <param name="contentTypeAlias"></param>
    /// <param name="parent"></param>
    /// <param name="culture"></param>
    /// <param name="preview"></param>
    /// <typeparam name="TCache"></typeparam>
    /// <returns></returns>
    public static IPublishedContent Generate<TCache>(
        this TCache cache,
        string name,
        string contentTypeAlias,
        IPublishedContent? parent = null,
        string culture = "",
        bool preview = false) where TCache : FakePublishedCache
    {
        var contentType = cache.GetContentType(contentTypeAlias) ?? cache.GenerateType(contentTypeAlias);

        var segment = ShortStringHelper.CleanStringForUrlSegment(name).ToLower();
        
        var content = new FakePublishedContent
        {
            Id = Interlocked.Increment(ref _id),
            Key = Guid.NewGuid(),
            Name = name,
            UrlSegment = segment,
            ContentType = contentType,
            Level = parent is null ? 1 : parent.Level + 1,
            Cultures = new Dictionary<string, PublishedCultureInfo>
            {
                { culture, new PublishedCultureInfo(culture, name, segment, DateTime.UtcNow) }
            },
        };

        cache.Add(content, parent?.Key, preview);

        return content;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="alias"></param>
    /// <typeparam name="TCache"></typeparam>
    public static IPublishedContentType GenerateType<TCache>(
        this TCache cache,
        string alias) where TCache : FakePublishedCache
    {
        var itemType = cache switch
        {
            IPublishedContentCache => PublishedItemType.Content,
            IPublishedMediaCache => PublishedItemType.Media,
            _ => PublishedItemType.Unknown,
        };

        var contentType = new FakePublishedContentType
        {
            Id = Interlocked.Increment(ref _id),
            Key = Guid.NewGuid(),
            Alias = alias,
            ItemType = itemType,
        };

        cache.AddType(contentType);

        return contentType;
    }
}