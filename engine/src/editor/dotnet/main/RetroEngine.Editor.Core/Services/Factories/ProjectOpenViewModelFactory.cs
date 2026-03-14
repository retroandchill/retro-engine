// // @file ProjectOpenViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton]
public class ProjectOpenViewModelFactory(IEnumerable<IProjectOpenViewTabFactory> factories)
    : IProjectOpenViewModelFactory
{
    private readonly ImmutableArray<IProjectOpenViewTabFactory> _factories = [.. factories];

    public ProjectOpenViewModel Create()
    {
        var model = new ProjectOpenViewModel();
        foreach (var tab in _factories.Select(x => x.Create()))
        {
            model.Tabs.Add(tab);
        }
        return model;
    }
}
