// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Our.UmbracoCms.InvisibleNodes.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;

namespace Our.UmbracoCms.InvisibleNodes
{
    public class InvisibleNodeComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterFor<IInvisibleNodeCache, InvisibleNodeCache>(Lifetime.Singleton);
            composition.RegisterFor<IInvisibleNodeRulesManager, InvisibleNodeRulesManager>(Lifetime.Singleton);

            composition.UrlProviders()
                .Append<InvisibleNodeUrlProvider>();

            composition.ContentFinders()
                .Append<InvisibleNodeContentFinder>();
        }
    }
}