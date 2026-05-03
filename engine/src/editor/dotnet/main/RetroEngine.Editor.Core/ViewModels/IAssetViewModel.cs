// // @file IAssetViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.Controls;

namespace RetroEngine.Editor.Core.ViewModels;

public interface IAssetViewModel : IDocument
{
    object Asset { get; }

    bool IsReadOnly { get; }
}
