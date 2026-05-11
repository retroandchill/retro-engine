// // @file DynamicContextMenu.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using RetroEngine.ToolMenu.Styling;

namespace RetroEngine.ToolMenu.Controls;

/// <summary>
/// Represents a <see cref="ContextMenu"/> with additional styles able to handle view models.
/// </summary>
/// <example>
/// &gt;DynamicContextMenu ItemsSource="{Binding MenuViewModels}" &lt;>
/// </example>
public class DynamicContextMenu : ContextMenu
{
    public DynamicContextMenu()
    {
        var style = new DynamicMenuStyles();

        Styles.Add(style);
    }

    protected override Type StyleKeyOverride => typeof(ContextMenu);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DynamicMenuItem();
    }
}
