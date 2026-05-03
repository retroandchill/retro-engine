// // @file TextureViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Strings;
using RetroEngine.Rendering;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class TextureViewModelFactory(
    IViewModelFactory<SceneViewModel> sceneFactory,
    ViewportManager viewportManager,
    AssetManager assetManager
) : IAssetViewModelFactory
{
    public Name AssetType => Texture.AssetType;

    public async ValueTask<IAssetViewModel> CreateViewModelAsync(
        AssetPath assetPath,
        CancellationToken cancellationToken = default
    )
    {
        var texture = await assetManager.LoadAssetAsync<Texture>(assetPath, cancellationToken);
        if (texture is null)
            throw new InvalidOperationException($"Failed to load texture asset at {assetPath}.");

        var scene = sceneFactory.CreateViewModel();
        var viewport = new Viewport(viewportManager) { Scene = scene.Scene };
        _ = new Sprite(scene.Scene) { Texture = texture };
        scene.Host.BindViewport(viewport);
        var nameAsString = assetPath.ToString();
        var lastDelimiter = nameAsString.LastIndexOf('/');
        return new TextureViewModel
        {
            Title = lastDelimiter >= 0 ? nameAsString[(lastDelimiter + 1)..] : nameAsString,
            Texture = texture,
            Scene = scene,
            Viewport = viewport,
        };
    }
}
