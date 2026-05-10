// // @file AvaloniaDynamicMenuService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace RetroEngine.ToolMenu.Services;

public class AvaloniaDynamicMenuServices : IStorageCommandFactoryService, IClipboardCommandFactoryService
{
    public static AvaloniaDynamicMenuServices Instance { get; } = new();

    private AvaloniaDynamicMenuServices() { }

    public ICommand CreateFolderPickerCommand<T>(
        ICommandFactoryService cmdf,
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> folderPickAsyncAction
    )
    {
        var options = new Avalonia.Platform.Storage.FolderPickerOpenOptions();
        configure(new StorageDialogConfiguration(options));
        return CreateTopLevelCommand(cmdf, top => top.FolderPickAsync(options, folderPickAsyncAction));
    }

    public ICommand CreateFileOpenCommand<T>(
        ICommandFactoryService cmdf,
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> openFileAsyncAction
    )
    {
        var options = new Avalonia.Platform.Storage.FilePickerOpenOptions();
        configure(new StorageDialogConfiguration(options));
        return CreateTopLevelCommand(cmdf, top => top.OpenFileAsync(options, openFileAsyncAction));
    }

    public ICommand CreateFileSaveCommand<T>(
        ICommandFactoryService cmdf,
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> saveFileAsyncAction
    )
    {
        var options = new Avalonia.Platform.Storage.FilePickerSaveOptions();
        configure(new StorageDialogConfiguration(options));
        return CreateTopLevelCommand(cmdf, top => top.SaveFileAsync(options, saveFileAsyncAction));
    }

    public ICommand CreateGetClipboardCommand<T>(ICommandFactoryService cmdf, Action<T> setValueFromClipboard)
    {
        if (typeof(T) == typeof(string))
        {
            void setText(string clipboardText)
            {
                if (clipboardText is T value)
                    setValueFromClipboard(value);
            }

            return CreateTopLevelCommand(cmdf, top => top.CopyTextFromClipboard(setText));
        }

        throw new NotImplementedException();
    }

    public ICommand CreateGetClipboardCommand<T>(ICommandFactoryService cmdf, Func<T, Task> setValueFromClipboard)
    {
        if (typeof(T) == typeof(string))
        {
            async Task setText(string clipboardText)
            {
                if (clipboardText is T value)
                    await setValueFromClipboard(value);
            }

            return CreateTopLevelCommand(cmdf, top => top.CopyTextFromClipboard(setText));
        }

        throw new NotImplementedException();
    }

    public ICommand CreateSetClipboardCommand<T>(ICommandFactoryService cmdf, Func<T?> getValueToCopyToClipboard)
    {
        if (typeof(T) == typeof(string))
        {
            string getText()
            {
                var value = getValueToCopyToClipboard();
                return value as string ?? string.Empty;
            }

            return CreateTopLevelCommand(cmdf, top => top.CopyTextToClipboard(getText));
        }

        throw new NotImplementedException();
    }

    public ICommand CreateSetClipboardCommand<T>(ICommandFactoryService cmdf, Func<Task<T?>> getValueToCopyToClipboard)
    {
        if (typeof(T) == typeof(string))
        {
            async Task<string> getText()
            {
                var value = await getValueToCopyToClipboard();
                return value as string ?? string.Empty;
            }

            return CreateTopLevelCommand(cmdf, top => top.CopyTextToClipboard(getText));
        }

        throw new NotImplementedException();
    }

    public ICommand CreateTopLevelCommand(ICommandFactoryService cmdf, Func<TopLevel, Task> topLevelAction)
    {
        return cmdf.CreateCommand<Visual?>(visual => topLevelAction(visual.GetActualTopLevel()!));
    }
}

public static class StorageDialogConfigurationExtensions
{
    public static StorageDialogConfiguration WithTitle(this StorageDialogConfiguration cfg, string title)
    {
        switch (cfg.Configuration)
        {
            case Avalonia.Platform.Storage.FilePickerOpenOptions read:
                read.Title = title;
                break;
            case Avalonia.Platform.Storage.FilePickerSaveOptions write:
                write.Title = title;
                break;
            case FolderPickerOpenOptions folder:
                folder.Title = title;
                break;
        }

        return cfg;
    }

    public static StorageDialogConfiguration WithAllFilesExt(this StorageDialogConfiguration cfg)
    {
        return WithExtension(cfg, "All Files", "*.*");
    }

    /// <summary>
    /// Adds an extension filter
    /// </summary>
    /// <param name="cfg">The target extension</param>
    /// <param name="name">The extension name</param>
    /// <param name="extensions">List of extensions in GLOB format. I.e. "*.png" or "*.*".</param>
    /// <returns>fluent</returns>
    public static StorageDialogConfiguration WithExtension(
        this StorageDialogConfiguration cfg,
        string name,
        params string[] extensions
    )
    {
        if (extensions.Length == 0)
            extensions = new string[] { "*.*" };

        var ft = new FilePickerFileType(name);
        ft.Patterns = extensions;

        static IReadOnlyList<FilePickerFileType> _add(
            IReadOnlyList<FilePickerFileType>? collection,
            FilePickerFileType item
        )
        {
            var list =
                collection as List<FilePickerFileType>
                ?? new List<FilePickerFileType>(collection ?? Enumerable.Empty<FilePickerFileType>());
            list.Add(item);
            return list;
        }

        switch (cfg.Configuration)
        {
            case Avalonia.Platform.Storage.FilePickerOpenOptions read:
                read.FileTypeFilter = _add(read.FileTypeFilter, ft);
                break;

            case Avalonia.Platform.Storage.FilePickerSaveOptions write:
                write.FileTypeChoices = _add(write.FileTypeChoices, ft);
                break;
            case Avalonia.Platform.Storage.FolderPickerOpenOptions:
                throw new InvalidOperationException("Not supported for folder picker");
        }

        return cfg;
    }
}
