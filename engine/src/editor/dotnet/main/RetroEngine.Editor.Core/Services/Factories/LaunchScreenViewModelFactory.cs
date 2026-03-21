// // @file LaunchScreenViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class LaunchScreenViewModelFactory(IEnumerable<IViewModelFactory<ILaunchScreenTabViewModel>> factories)
    : ViewModelFactory<LaunchScreenViewModel>
{
    private readonly ImmutableArray<IViewModelFactory<ILaunchScreenTabViewModel>> _factories = [.. factories];

    public override LaunchScreenViewModel CreateViewModel()
    {
        var model = new LaunchScreenViewModel();
        foreach (var factory in _factories)
        {
            model.Tabs.Add(factory.CreateViewModel());
        }

        return model;
    }
}
