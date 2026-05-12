// // @file TextureViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.AssetTools;
using RetroEngine.AssetTools.ViewModels;
using RetroEngine.Core.Math;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Rendering;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class TextureAssetActions(
    IViewModelFactory<SceneViewModel> sceneFactory,
    ViewportManager viewportManager,
    RenderManager renderManager
) : IAssetTypeActions
{
    public Type SupportedType => typeof(Texture);

    public IAssetViewModel CreateViewModel(AssetPath assetPath, object asset)
    {
        if (asset is not Texture texture)
            throw new InvalidOperationException($"Asset at {assetPath} is not a Texture.");

        var scene = sceneFactory.CreateViewModel();
        scene.Width = texture.Width;
        scene.Height = texture.Height;
        var viewport = new Viewport(viewportManager) { Scene = scene.Scene, CameraPivot = new Vector2F(0.5f, 0.5f) };
        _ = new Sprite(scene.Scene) { Texture = texture, Pivot = new Vector2F(0.5f, 0.5f) };
        renderManager.BindViewportToWindow(viewport, 0);
        scene.Host.OnWindowCreated += windowId =>
        {
            if (viewport.Disposed)
                return;

            renderManager.BindViewportToWindow(viewport, windowId);
        };
        scene.Host.OnWindowDestroyed += _ =>
        {
            if (viewport.Disposed)
                return;

            renderManager.BindViewportToWindow(viewport, 0);
        };
        return new TextureViewModel
        {
            Path = assetPath,
            Texture = texture,
            Scene = scene,
            Viewport = viewport,
        };
    }
}
