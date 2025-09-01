using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakePublishedContentCache : FakePublishedCache, IPublishedContentCache
{
    public Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null) =>
        Task.FromResult(GetById(preview ?? false, id));

    public Task<IPublishedContent?> GetByIdAsync(Guid key, bool? preview = null) =>
        Task.FromResult(GetById(preview ?? false, key));

    public IPublishedContent? GetByRoute(bool preview, string route, bool? hideTopLevelNode = null,
        string? culture = null)
    {
        throw new NotImplementedException();
    }

    public IPublishedContent? GetByRoute(string route, bool? hideTopLevelNode = null, string? culture = null)
    {
        throw new NotImplementedException();
    }

    public string? GetRouteById(bool preview, int contentId, string? culture = null)
    {
        throw new NotImplementedException();
    }

    public string? GetRouteById(int contentId, string? culture = null)
    {
        throw new NotImplementedException();
    }
}