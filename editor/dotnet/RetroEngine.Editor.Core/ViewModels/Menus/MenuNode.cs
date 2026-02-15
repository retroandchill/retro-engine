// // @file MenuNode.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.ViewModels.Menus;

public abstract class MenuNode(Name id) : ObservableObject
{
    public Name Id { get; } = id;

    public bool IsVisible { get; set; } = true;
}
