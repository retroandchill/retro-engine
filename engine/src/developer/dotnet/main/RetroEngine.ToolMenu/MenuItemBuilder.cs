// // @file MenuItemBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows.Input;
using RetroEngine.ToolMenu.Services;
using RetroEngine.ToolMenu.ViewModel;

namespace RetroEngine.ToolMenu;

[DebuggerDisplay("{GetDebuggerDisplay()}")]
public sealed class MenuItemBuilder
{
    #region debugging

    internal string GetDebuggerDisplay()
    {
        return _factory?.GetDebuggerDisplay() ?? $"{Icon} {Header}";
    }

    #endregion

    #region lifecycle

    internal MenuItemBuilder(MenuBuilder context)
    {
        _context = context;
        Icon = null;
        Header = null;
        ToolTip = null;
        _factory = null;
    }

    internal void MakeSeparator()
    {
        _factory = new MenuItemSeparatorViewModel.Factory(this);
    }

    internal void MakeSection(string? sectionName = null)
    {
        _factory = new MenuItemSectionViewModel.Factory(this, sectionName);
    }

    internal void MakeGroup(MenuBuilder subMenu)
    {
        _factory = new MenuItemGroupViewModel.Factory(this, subMenu);
    }

    #endregion

    #region data

    private readonly MenuBuilder _context;

    public object? Icon { get; set; }

    public object? Header { get; set; }

    public object? ToolTip { get; set; }

    private MenuItemViewModelFactory? _factory;

    #endregion

    #region fluent API

    public MenuItemBuilder WithIcon(object? icon)
    {
        Icon = icon;
        return this;
    }

    public MenuItemBuilder WithHeader(object? header)
    {
        Header = header;
        return this;
    }

    public MenuItemBuilder WithToolTip(object? toolTip)
    {
        ToolTip = toolTip;
        return this;
    }

    public MenuItemBuilder WithCheckBox(Func<bool> getter, Action<bool> setter)
    {
        return WithCustomMenuItemFactory(new MenuItemCheckBoxViewModel.Factory(this, getter, setter));
    }

    public MenuItemBuilder WithRadioButton(string groupName, Func<bool> getter, Action<bool> setter)
    {
        return WithCustomMenuItemFactory(new MenuItemRadioButtonViewModel.Factory(this, groupName, getter, setter));
    }

    public MenuItemBuilder WithCommand(Action action)
    {
        return WithCommand(CreateCommand(action));
    }

    public MenuItemBuilder WithCommand(Func<Task> action)
    {
        return WithCommand(CreateCommand(action));
    }

    public MenuItemBuilder WithCommand(ICommand command)
    {
        return WithCustomMenuItemFactory(new MenuItemCommandViewModel.Factory(this, command));
    }

    public MenuItemBuilder WithCommand<T>(Action<T?> action, T? parameter) =>
        WithCommand(CreateCommand(action), parameter);

    public MenuItemBuilder WithCommand<T>(Func<T?, Task> action, T? parameter) =>
        WithCommand(CreateCommand(action), parameter);

    public MenuItemBuilder WithCommand<T>(ICommand command, T? parameter)
    {
        return WithCustomMenuItemFactory(new MenuItemParamCommandViewModel<T>.Factory(this, command, parameter));
    }

    public MenuItemBuilder WithFolderPicker<T>(
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> folderPickAction
    )
    {
        var cmd = _context._CreateFolderPickCommand(configure, folderPickAction);
        return WithCustomMenuItemFactory(new MenuItemSelfCommandViewModel.Factory(this, cmd));
    }

    public MenuItemBuilder WithFileOpen<T>(Action<StorageDialogConfiguration> configure, Func<T, Task> openFileAction)
    {
        var cmd = _context._CreateFileOpenCommand(configure, openFileAction);
        return WithCustomMenuItemFactory(new MenuItemSelfCommandViewModel.Factory(this, cmd));
    }

    public MenuItemBuilder WithFileSave<T>(Action<StorageDialogConfiguration> configure, Func<T, Task> saveFileAction)
    {
        var cmd = _context._CreateFileSaveCommand(configure, saveFileAction);
        return WithCustomMenuItemFactory(new MenuItemSelfCommandViewModel.Factory(this, cmd));
    }

    public MenuItemBuilder WithCopyFromClipboard<T>(Action<T> setValueFromClipboard)
    {
        var cmd = _context._CreateGetClipboardCommand(setValueFromClipboard);
        return WithCustomMenuItemFactory(new MenuItemSelfCommandViewModel.Factory(this, cmd));
    }

    public MenuItemBuilder WithCopyToClipboard<T>(Func<T?> getValueToCopyToClipboard)
    {
        var cmd = _context._CreateSetClipboardCommand(getValueToCopyToClipboard);
        return WithCustomMenuItemFactory(new MenuItemSelfCommandViewModel.Factory(this, cmd));
    }

    public MenuItemBuilder WithTextToClipboard(Func<string> copyTextAction)
    {
        throw new NotImplementedException();
    }

    public MenuItemBuilder WithCustomMenuItemFactory(MenuItemViewModelFactory factory)
    {
        _factory = factory;
        return this;
    }

    #endregion

    #region API

    private ICommand CreateCommand(Action action) => _context.CreateCommand(action);

    private ICommand CreateCommand<T>(Action<T?> action) => _context.CreateCommand(action);

    private ICommand CreateCommand(Func<Task> action) => _context.CreateCommand(action);

    private ICommand CreateCommand<T>(Func<T?, Task> action) => _context.CreateCommand(action);

    public IMenuItemViewModel? CreateMenuItem()
    {
        if (_factory == null)
            throw new InvalidOperationException("You must register an action on this item before evaluation.");

        return _factory.Create();
    }

    #endregion
}
