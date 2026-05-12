// // @file DynamicMenuItem.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using RetroEngine.ToolMenu.Styling;
using RetroEngine.ToolMenu.ViewModel;

namespace RetroEngine.ToolMenu.Controls;

/// <summary>
/// Represents a <see cref="MenuItem"/> with additional styles able to handle view models.
/// </summary>
/// <example>
/// &gt;MenuItem ItemsSource="{Binding MenuViewModels}" &lt;>
/// </example>
public class DynamicMenuItem : MenuItem
{
    public DynamicMenuItem()
    {
        var style = new DynamicMenuStyles();

        Styles.Add(style);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DynamicMenuItem();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        UpdateClasses();
    }

    private void UpdateClasses()
    {
        var item = DataContext;

        Classes.Set("separator", item is IMenuSeparator);
        Classes.Set("section", item is IMenuSectionHeader);
        Classes.Set("enableable", item is IEnableableMenuItem);
        Classes.Set("headered", item is IHeaderedMenuItem);
        Classes.Set("tooltip", item is IToolTipMenuItem { ToolTip.IsEmptyOrWhiteSpace: false });
        Classes.Set("icon", item is IIconMenuItem);
        Classes.Set("hotkey", item is IHotKeyMenuItem);
        Classes.Set("command", item is ICommandMenuItem);
        Classes.Set("self", item is ISelfCommandMenuItem);
        Classes.Set("parameter", item is ICommandParameterMenuItem);
        Classes.Set("sub", item is ISubMenuItem);
        Classes.Set("checkable", item is ICheckableMenuItem);
        Classes.Set("radio", item is IRadioButtonMenuItem);
        Classes.Set("checkbox", item is ICheckBoxMenuItem);
    }

    protected override Type StyleKeyOverride => typeof(MenuItem);
}
