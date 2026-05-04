// // @file TextureViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.RetroEngine.Controls;
using RetroEngine.Assets;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Rendering;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<TextureView>]
public sealed partial class TextureViewModel : Document, IAssetViewModel
{
    public bool IsReadOnly => true;

    public required AssetPath Path { get; init; }
    object IAssetViewModel.Asset => Texture;

    public required Texture Texture { get; init; }

    public required SceneViewModel Scene { get; init; }

    public required Viewport Viewport { get; init; }

    public TextureViewModel()
    {
        CanClose = true;
    }

    public override bool OnClose()
    {
        var result = base.OnClose();
        Viewport.Dispose();
        Scene.Dispose();

        return result;
    }
}
