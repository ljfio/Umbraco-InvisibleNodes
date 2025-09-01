using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public sealed class FakeDomainCache : IDomainCache
{
    private int _id = 1;
    private readonly IList<Domain> _domains = new List<Domain>();

    public FakeDomainCache(string defaultCulture)
    {
        DefaultCulture = defaultCulture;
    }

    public string DefaultCulture { get; }

    public IEnumerable<Domain> GetAll(bool includeWildcards) =>
        includeWildcards
            ? _domains
            : _domains.Where(d => d.IsWildcard == false);

    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false) =>
        includeWildcards
            ? _domains.Where(d => d.ContentId == documentId)
            : _domains.Where(d => d.ContentId == documentId && d.IsWildcard == false);

    public bool HasAssigned(int documentId, bool includeWildcards = false) =>
        includeWildcards
            ? _domains.Any(d => d.ContentId == documentId)
            : _domains.Any(d => d.ContentId == documentId && d.IsWildcard == false);

    /// <summary>
    /// Adds a domain to the cache
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="url"></param>
    /// <param name="culture"></param>
    public Domain Add(int contentId, string url, string? culture = null)
    {
        var id = Interlocked.Increment(ref _id);
        var domain = new Domain(id, url, contentId, culture, false, id);
        
        _domains.Add(domain);
        
        return domain;
    }

    /// <summary>
    /// Removes a domain from the cache by <paramref name="id"/>
    /// </summary>
    /// <param name="id"></param>
    public void Remove(int id)
    {
        var domain = _domains.FirstOrDefault(d => d.Id == id);

        if (domain is not null)
            _domains.Remove(domain);
    }
}

public static class FakeDomainCacheExtensions
{
    /// <summary>
    /// Adds multiple domains to the cache
    /// </summary>
    /// <param name="domainCache"></param>
    /// <param name="contentId"></param>
    /// <param name="culture"></param>
    /// <param name="urls"></param>
    public static IEnumerable<Domain> AddRange(this FakeDomainCache domainCache, int contentId, params string[] urls)
    {
        var domains = new List<Domain>();
        
        foreach (var url in urls)
            domains.Add(domainCache.Add(contentId, url, domainCache.DefaultCulture));

        return domains;
    }
}