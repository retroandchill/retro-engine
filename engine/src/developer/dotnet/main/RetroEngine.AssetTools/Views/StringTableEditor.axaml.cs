// @file StringTableEditor.axaml.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RetroEngine.Assets;
using RetroEngine.AssetTools.ViewModels;
using RetroEngine.Portable.Localization.StringTables;

namespace RetroEngine.AssetTools.Views;

public partial class StringTableEditor : UserControl
{
    public StringTableEditor()
    {
        InitializeComponent();
    }
}
