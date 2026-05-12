// // @file MenuItemViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableCollections;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.ToolMenu.ViewModel;

public abstract partial class MenuItemBase(Name id) : ObservableObject, IMenuItemEntry
{
    public Name Id { get; } = id;
}

public sealed class MenuItemSeparator() : MenuItemBase(Name.None), IMenuSeparator;

public sealed partial class MenuSectionHeader(Name id, Text sectionName) : MenuItemBase(id), IMenuSectionHeader
{
    [ObservableProperty]
    public partial Text SectionName { get; set; } = sectionName;
}

public abstract partial class MenuItemEntryBase(Name id, Text header)
    : MenuItemBase(id),
        IEnableableMenuItem,
        IHeaderedMenuItem,
        IToolTipMenuItem,
        IIconMenuItem
{
    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial Text Header { get; set; } = header;

    [ObservableProperty]
    public partial Text ToolTip { get; set; }

    [ObservableProperty]
    public partial object? Icon { get; set; }
}

public partial class MenuCommand(Name id, Text header, ICommand command)
    : MenuItemEntryBase(id, header),
        ICommandMenuItem,
        IHotKeyMenuItem
{
    [ObservableProperty]
    public partial ICommand Command { get; set; } = command;

    [ObservableProperty]
    public partial KeyGesture? HotKey { get; set; }
}

public sealed class SelfParamMenuCommand(Name id, Text header, ICommand command)
    : MenuCommand(id, header, command),
        ISelfCommandMenuItem;

public sealed partial class ParameterizedMenuCommand(Name id, Text header, ICommand command, object? parameter)
    : MenuCommand(id, header, command),
        ICommandParameterMenuItem
{
    [ObservableProperty]
    public partial object? Parameter { get; set; } = parameter;
}

public sealed class SubMenu(Name id, Text header) : MenuItemEntryBase(id, header), ISubMenuItem
{
    private readonly ObservableList<IMenuItemEntry> _children = [];
    public IReadOnlyObservableList<IMenuItemEntry> Children => _children;

    public IMenuItemEntry? FindItem(Name id)
    {
        return _children.FirstOrDefault(item => item.Id == id);
    }

    public void Add(IMenuItemEntry item)
    {
        _children.Add(item);
    }

    public void AddRange(IEnumerable<IMenuItemEntry> items)
    {
        _children.AddRange(items);
    }

    public void AddRange(params ReadOnlySpan<IMenuItemEntry> items)
    {
        _children.AddRange(items);
    }

    public void Insert(int index, IMenuItemEntry item)
    {
        _children.Insert(index, item);
    }

    public void InsertRange(int index, IEnumerable<IMenuItemEntry> items)
    {
        _children.InsertRange(index, items);
    }

    public void InsertRange(int index, params ReadOnlySpan<IMenuItemEntry> items)
    {
        _children.InsertRange(index, items);
    }

    public void Remove(Name id)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            var item = _children[i];
            if (item.Id != id)
                continue;
            _children.RemoveAt(i);
            return;
        }
    }

    public void Remove(IMenuItemEntry item)
    {
        _children.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _children.RemoveAt(index);
    }

    public void RemoveRange(int index, int count)
    {
        _children.RemoveRange(index, count);
    }

    public void Clear()
    {
        _children.Clear();
    }
}

public abstract partial class CheckableMenuItem(Name id, Text header)
    : MenuItemEntryBase(id, header),
        ICheckableMenuItem
{
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
}

public sealed class CheckBoxMenuItem(Name id, Text header) : CheckableMenuItem(id, header), ICheckBoxMenuItem;

public sealed partial class RadioButtonMenuItem(Name id, Text header, string? groupName)
    : CheckableMenuItem(id, header),
        IRadioButtonMenuItem
{
    [ObservableProperty]
    public partial string? GroupName { get; set; } = groupName;
}
