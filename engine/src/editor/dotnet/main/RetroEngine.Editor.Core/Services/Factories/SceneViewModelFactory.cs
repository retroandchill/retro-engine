// // @file SceneViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class SceneViewModelFactory(
    SceneManager sceneManager,
    IViewModelFactory<EngineRendererProvider> engineControlHost
) : ViewModelFactory<SceneViewModel>
{
    public override SceneViewModel CreateViewModel()
    {
        var scene = new Scene(sceneManager);
        var host = engineControlHost.CreateViewModel();
        return new SceneViewModel { Host = host, Scene = scene };
    }
}
