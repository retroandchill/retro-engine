// // @file MenuBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows.Input;
using RetroEngine.ToolMenu.Services;
using RetroEngine.ToolMenu.ViewModel;

namespace RetroEngine.ToolMenu;

public sealed class MenuBuilder
{
    #region lifecycle

    public static void RegisterCommandsFactory(object commandsFactory)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (commandsFactory is ICommandFactoryService cf)
            _defaultCommandFactory = cf;
        if (commandsFactory is IStorageCommandFactoryService scf)
            _defaultStorageCommandFactory = scf;
        if (commandsFactory is IClipboardCommandFactoryService ccf)
            _defaultClipboardCommandFactory = ccf;
    }

    public MenuBuilder() { }

    public MenuBuilder(ICommandFactoryService commandsFactory)
    {
        _commandFactory = commandsFactory;
    }

    private MenuBuilder CreateMenuBuilder()
    {
        return new MenuBuilder(this);
    }

    private MenuBuilder(MenuBuilder parent)
    {
        _parent = parent;
    }

    #endregion

    #region data

    private readonly MenuBuilder? _parent;

    private readonly ICommandFactoryService? _commandFactory;
    private static ICommandFactoryService? _defaultCommandFactory;
    private static IStorageCommandFactoryService? _defaultStorageCommandFactory;
    private static IClipboardCommandFactoryService? _defaultClipboardCommandFactory;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly List<MenuItemBuilder> _items = [];

    #endregion

    #region properties

    public int Count => _items.Count;

    #endregion

    #region API

    public MenuItemBuilder Append(object? header)
    {
        var builder = CreateMenuItemBuilder();
        builder.Header = header;
        _items.Add(builder);
        return builder;
    }

    public MenuItemBuilder Append(object? icon, object? header)
    {
        var builder = CreateMenuItemBuilder();
        builder.Icon = icon;
        builder.Header = header;
        _items.Add(builder);
        return builder;
    }

    public MenuBuilder AppendGroup(object? header)
    {
        return AppendGroup(null, header);
    }

    public MenuBuilder AppendGroup(object? icon, object? header)
    {
        var subMenu = CreateMenuBuilder();
        return AppendGroup(icon, header, subMenu);
    }

    public MenuBuilder AppendGroup(object? icon, object? header, MenuBuilder subMenu)
    {
        if (subMenu == this)
            throw new ArgumentException("Cannot add this as menu.");

        var builder = CreateMenuItemBuilder();
        builder.Icon = icon;
        builder.Header = header;
        builder.MakeGroup(subMenu);
        _items.Add(builder);
        return subMenu;
    }

    public void AppendSeparator()
    {
        var builder = CreateMenuItemBuilder();
        builder.MakeSeparator();
        _items.Add(builder);
    }

    public void AppendSection(string? sectionName)
    {
        var builder = CreateMenuItemBuilder();
        builder.MakeSection(sectionName);
        _items.Add(builder);
    }

    public IMenuItemViewModel ToGroup(object? icon, object? header)
    {
        return new MenuItemGroupViewModel(icon, header, EnumerateMenuItems());
    }

    public IEnumerable<IMenuItemViewModel> EnumerateMenuItems()
    {
        return _items.Select(item => item.CreateMenuItem()).OfType<IMenuItemViewModel>();
    }

    #endregion

    #region commands factory

    private MenuItemBuilder CreateMenuItemBuilder()
    {
        return new MenuItemBuilder(this);
    }

    internal ICommand CreateCommand(Action action) => GetCommandFactory().CreateCommand(action);

    internal ICommand CreateCommand<T>(Action<T?> action) => GetCommandFactory().CreateCommand(action);

    internal ICommand CreateCommand(Func<Task> action) => GetCommandFactory().CreateCommand(action);

    internal ICommand CreateCommand<T>(Func<T?, Task> action) => GetCommandFactory().CreateCommand(action);

    private ICommandFactoryService GetCommandFactory()
    {
        return _parent?.GetCommandFactory()
            ?? _commandFactory
            ?? _defaultCommandFactory
            ?? throw new NotImplementedException(
                $"{nameof(ICommandFactoryService)} not found.\r\nEither pass one to {nameof(MenuBuilder)}'s constructor,\r\nor register a default one with {nameof(MenuBuilder)}.{nameof(RegisterCommandsFactory)} at application startup."
            );
    }

    internal ICommand _CreateFolderPickCommand<T>(
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> folderPickAction
    ) => GetStorageCommandFactory().CreateFolderPickerCommand(GetCommandFactory(), configure, folderPickAction);

    internal ICommand _CreateFileOpenCommand<T>(
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> fileOpenAction
    ) => GetStorageCommandFactory().CreateFileOpenCommand(GetCommandFactory(), configure, fileOpenAction);

    internal ICommand _CreateFileSaveCommand<T>(
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> fileSaveAction
    ) => GetStorageCommandFactory().CreateFileSaveCommand(GetCommandFactory(), configure, fileSaveAction);

    private IStorageCommandFactoryService GetStorageCommandFactory()
    {
        return _parent?.GetStorageCommandFactory()
            ?? _defaultStorageCommandFactory
            ?? throw new NotImplementedException(
                $"{nameof(IStorageCommandFactoryService)} not found.\r\nEither pass one to {nameof(MenuBuilder)}'s constructor,\r\nor register a default one with {nameof(MenuBuilder)}.{nameof(RegisterCommandsFactory)} at application startup."
            );
    }

    internal ICommand _CreateGetClipboardCommand<T>(Action<T> setValueFromClipboard) =>
        GetClipboardCommandFactory().CreateGetClipboardCommand(GetCommandFactory(), setValueFromClipboard);

    internal ICommand _CreateSetClipboardCommand<T>(Func<T?> getValueToCopyToClipboard) =>
        GetClipboardCommandFactory().CreateSetClipboardCommand(GetCommandFactory(), getValueToCopyToClipboard);

    private IClipboardCommandFactoryService GetClipboardCommandFactory()
    {
        return _parent?.GetClipboardCommandFactory()
            ?? _defaultClipboardCommandFactory
            ?? throw new NotImplementedException(
                $"{nameof(IClipboardCommandFactoryService)} not found.\r\nEither pass one to {nameof(MenuBuilder)}'s constructor,\r\nor register a default one with {nameof(MenuBuilder)}.{nameof(RegisterCommandsFactory)} at application startup."
            );
    }

    #endregion
}
