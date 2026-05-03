// // @file TextureViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.RetroEngine.Controls;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<TextureView>]
public sealed partial class TextureViewModel : Document, IDisposable
{
    public required SceneViewModel Scene { get; init; }

    public required Viewport Viewport { get; init; }

    public void Dispose()
    {
        Viewport.Dispose();
        Scene.Dispose();
    }
}
