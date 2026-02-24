// // @file ViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;

namespace RetroEngine.Editor.Core.ViewModels;

public interface IViewModel
{
    Control CreateView();
}
