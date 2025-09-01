using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakePublishedMediaCache : FakePublishedCache, IPublishedMediaCache
{
    public Task<IPublishedContent?> GetByIdAsync(int id) => Task.FromResult(GetById(id));

    public Task<IPublishedContent?> GetByIdAsync(Guid key) => Task.FromResult(GetById(key));
}