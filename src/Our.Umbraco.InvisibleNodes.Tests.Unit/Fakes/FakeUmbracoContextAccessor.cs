using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.InvisibleNodes.Tests.Unit.Fakes;

public class FakeUmbracoContextAccessor : IUmbracoContextAccessor
{
    private IUmbracoContext? _umbracoContext;
    
    public bool TryGetUmbracoContext([MaybeNullWhen(false)] out IUmbracoContext umbracoContext)
    {
        umbracoContext = _umbracoContext;
        return _umbracoContext is not null;
    }

    public void Clear() => _umbracoContext = null;

    public void Set(IUmbracoContext umbracoContext) => _umbracoContext = umbracoContext;
}