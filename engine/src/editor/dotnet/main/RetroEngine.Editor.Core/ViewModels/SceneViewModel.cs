// // @file SceneViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.ViewModels;

[ViewModelFor<SceneView>]
public sealed partial class SceneViewModel : ObservableObject, IDisposable
{
    public required EngineRendererProvider Host { get; init; }

    public required Scene Scene { get; init; }

    public void Dispose()
    {
        Scene.Dispose();
    }
}
