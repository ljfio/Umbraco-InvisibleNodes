using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakePublishedCache : IPublishedCache
{
    private readonly FakeDocumentNavigationService _navigationService;

    public FakePublishedCache() => _navigationService = new(this);

    public IDocumentNavigationQueryService DocumentNavigationQueryService => _navigationService;

    protected IList<IPublishedContent> Cache { get; } = new List<IPublishedContent>();

    protected IList<IPublishedContent> PreviewCache { get; } = new List<IPublishedContent>();

    protected IList<IPublishedContentType> TypeCache { get; } = new List<IPublishedContentType>();


    public IPublishedContent? GetById(bool preview, int contentId) => GetBy(preview, c => c.Id == contentId);

    public IPublishedContent? GetById(bool preview, Guid contentId) => GetBy(preview, c => c.Key == contentId);

    public IPublishedContent? GetById(bool preview, Udi contentId)
    {
        if (contentId is GuidUdi guidUdi)
            return GetBy(preview, c => c.Key == guidUdi.Guid);

        if (contentId is StringUdi stringUdi)
        {
            if (Guid.TryParse(stringUdi.Id, out var guid))
                return GetBy(preview, c => c.Key == guid);

            if (int.TryParse(stringUdi.Id, out var id))
                return GetBy(preview, c => c.Id == id);
        }

        return null;
    }

    public IPublishedContent? GetById(int contentId) => GetById(false, contentId);

    public IPublishedContent? GetById(Guid contentId) => GetById(false, contentId);

    public IPublishedContent? GetById(Udi contentId) => GetById(false, contentId);

    protected IPublishedContent? GetBy(bool preview, Func<IPublishedContent, bool> predicate)
    {
        if (preview)
        {
            var item = PreviewCache.FirstOrDefault(predicate);

            if (item is not null)
                return item;
        }

        return Cache.FirstOrDefault(predicate);
    }

    public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        // TODO Handle preview
        // TODO Handle culture

        if (!DocumentNavigationQueryService.TryGetRootKeys(out var rootKeys))
            return [];

        return rootKeys
            .Select(GetById)
            .Where(c => c is not null)
            .ToList()!;
    }

    public IEnumerable<IPublishedContent> GetAtRoot(string? culture = null) => GetAtRoot(false, culture);

    public bool HasContent(bool preview) => (preview && PreviewCache.Count is not 0) || Cache.Count is not 0;

    public bool HasContent() => HasContent(false);

    public IPublishedContentType? GetContentType(int id) => GetContentTypeBy(t => t.Id == id);

    public IPublishedContentType? GetContentType(string alias) => GetContentTypeBy(t => t.Alias == alias);

    public IPublishedContentType? GetContentType(Guid key) => GetContentTypeBy(t => t.Key == key);

    protected IPublishedContentType? GetContentTypeBy(Func<IPublishedContentType, bool> predicate) =>
        TypeCache.FirstOrDefault(predicate);

    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType) =>
        Cache.Where(c => c.ContentType == contentType);

    public void Add(IPublishedContent content, Guid? parentKey = null, bool isPreview = false)
    {
        if (isPreview)
            PreviewCache.Add(content);
        else
            Cache.Add(content);

        _navigationService.Add(content.Key, content.ContentType.Key, parentKey);
    }

    public void AddType(IPublishedContentType contentType) => TypeCache.Add(contentType);
}