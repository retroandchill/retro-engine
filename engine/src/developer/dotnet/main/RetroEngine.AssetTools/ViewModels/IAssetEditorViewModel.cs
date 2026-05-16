// @file IAssetEditorViewModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.AssetTools.ViewModels;

public interface IAssetEditorViewModel : IAssetViewModel
{
    event Action? AssetChanged;
}

public interface IAssetEditorViewModel<out T> : IAssetViewModel<T>, IAssetEditorViewModel
    where T : class;
