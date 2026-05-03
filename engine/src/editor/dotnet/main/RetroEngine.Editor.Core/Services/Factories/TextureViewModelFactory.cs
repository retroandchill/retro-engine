// // @file TextureViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class TextureViewModelFactory(
    IViewModelFactory<SceneViewModel> sceneFactory,
    ViewportManager viewportManager
) : ViewModelFactory<TextureViewModel>
{
    public override TextureViewModel CreateViewModel()
    {
        var scene = sceneFactory.CreateViewModel();
        var viewport = new Viewport(viewportManager) { Scene = scene.Scene };
        scene.Host.BindViewport(viewport);
        return new TextureViewModel { Scene = scene, Viewport = viewport };
    }
}
