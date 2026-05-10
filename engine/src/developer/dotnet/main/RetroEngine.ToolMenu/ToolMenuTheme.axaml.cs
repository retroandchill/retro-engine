// // @file ToolMenuTheme.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace RetroEngine.ToolMenu;

public class ToolMenuTheme : Styles
{
    public ToolMenuTheme(IServiceProvider? services = null)
    {
        AvaloniaXamlLoader.Load(services, this);
    }
}
