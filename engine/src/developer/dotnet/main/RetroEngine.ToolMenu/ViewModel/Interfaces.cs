// // @file IMenuItemEntry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;
using Avalonia.Input;
using ObservableCollections;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemEntry
{
    Name Id { get; }

    bool IsVisible { get; set; }
}

public interface IMenuSeparator : IMenuItemEntry;

public interface IMenuSectionHeader : IMenuItemEntry
{
    Text SectionName { get; set; }
}

public interface IEnableableMenuItem : IMenuItemEntry
{
    bool IsEnabled { get; set; }
}

public interface IHeaderedMenuItem : IMenuItemEntry
{
    Text Header { get; set; }
}

public interface IIconMenuItem : IMenuItemEntry
{
    object? Icon { get; set; }
}

public interface IToolTipMenuItem : IMenuItemEntry
{
    Text ToolTip { get; set; }
}

public interface IHotKeyMenuItem : IMenuItemEntry
{
    KeyGesture? HotKey { get; set; }
}

public interface ICommandMenuItem : IMenuItemEntry
{
    ICommand Command { get; set; }
}

public interface ISelfCommandMenuItem : ICommandMenuItem;

public interface ICommandParameterMenuItem : ICommandMenuItem
{
    object? Parameter { get; set; }
}

public interface ISubMenuItem : IMenuItemEntry
{
    IReadOnlyObservableList<IMenuItemEntry> Children { get; }

    IMenuItemEntry? FindItem(Name id);

    void Add(IMenuItemEntry item);

    void AddRange(IEnumerable<IMenuItemEntry> items);

    void AddRange(params ReadOnlySpan<IMenuItemEntry> items);

    void Insert(int index, IMenuItemEntry item);

    void InsertRange(int index, IEnumerable<IMenuItemEntry> items);

    void InsertRange(int index, params ReadOnlySpan<IMenuItemEntry> items);

    void Remove(Name id);

    void Remove(IMenuItemEntry item);

    void RemoveAt(int index);

    void RemoveRange(int index, int count);

    void Clear();
}

public interface ICheckableMenuItem : IMenuItemEntry
{
    bool IsChecked { get; set; }
}

public interface IRadioButtonMenuItem : ICheckableMenuItem
{
    string? GroupName { get; set; }
}

public interface ICheckBoxMenuItem : ICheckableMenuItem;
