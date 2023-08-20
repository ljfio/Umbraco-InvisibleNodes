// Copyright 2023 Luke Fisher
// SPDX-License-Identifier: Apache-2.0

using Our.UmbracoCms.VirtualNodes.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;

namespace Our.UmbracoCms.VirtualNodes
{
    public class VirtualNodeComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterFor<IVirtualNodeCache, VirtualNodeCache>(Lifetime.Singleton);
            composition.RegisterFor<IVirtualNodeRulesManager, VirtualNodeRulesManager>(Lifetime.Singleton);

            composition.ContentFinders()
                .Append<VirtualNodeContentFinder>();
        }
    }
}