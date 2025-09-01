using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakePublishedContent : IPublishedContent
{
    public int Id { get; set; }
    public Guid Key { get; set; }

    public string Name { get; set; }
    public string? UrlSegment { get; set; }
    public int SortOrder { get; set; }
    public int Level { get; set; }
    public string Path { get; set; }
    public int? TemplateId { get; set; }
    public int CreatorId { get; set; }
    public DateTime CreateDate { get; set; }
    public int WriterId { get; set; }
    public DateTime UpdateDate { get; set; }
    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; set; }
    public PublishedItemType ItemType { get; set; }

    public IPublishedContent? Parent => throw new InvalidOperationException();
    public IEnumerable<IPublishedContent> Children => throw new InvalidOperationException();

    public IPublishedContentType ContentType { get; set; } = null!;
    public IEnumerable<IPublishedProperty> Properties { get; set; } = [];

    public IPublishedProperty? GetProperty(string alias) => Properties.FirstOrDefault(p => p.Alias.Equals(alias));
    public bool IsDraft(string? culture = null) => false;
    public bool IsPublished(string? culture = null) => true;
}