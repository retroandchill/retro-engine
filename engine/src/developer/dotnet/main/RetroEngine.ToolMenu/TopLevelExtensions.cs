// // @file TopLevelExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace RetroEngine.ToolMenu;

internal static class TopLevelExtensions
{
    extension(TopLevel top)
    {
        internal async Task FolderPickAsync<T>(FolderPickerOpenOptions? options, Func<T, Task> folderPickAsyncAction)
        {
            options ??= new FolderPickerOpenOptions { Title = "Select Directory" };

            options.AllowMultiple = false;

            var folders = await top.StorageProvider.OpenFolderPickerAsync(options);
            if (!options.AllowMultiple && folders.Count == 1)
                await ProcessSingleFolderPickAsync(folders[0], folderPickAsyncAction);

            foreach (var f in folders)
            {
                f.Dispose();
            }
        }

        internal async Task OpenFileAsync<T>(FilePickerOpenOptions? options, Func<T, Task> openFileAsyncAction)
        {
            options ??= new FilePickerOpenOptions();

            options.AllowMultiple = false;

            var files = await top.StorageProvider.OpenFilePickerAsync(options);
            if (!options.AllowMultiple && files.Count == 1)
                await ProcessSingleFilePickAsync(files[0], openFileAsyncAction);

            foreach (var f in files)
            {
                f.Dispose();
            }
        }

        internal async Task SaveFileAsync<T>(FilePickerSaveOptions? options, Func<T, Task> saveFileAsyncAction)
        {
            options ??= new FilePickerSaveOptions();

            var file = await top.StorageProvider.SaveFilePickerAsync(options);
            if (file == null)
                return;

            await ProcessSingleFilePickAsync(file, saveFileAsyncAction);

            file.Dispose();
        }

        internal async Task CopyTextFromClipboard(Action<string> textSetter)
        {
            var cb = top.Clipboard;
            if (cb == null)
                return;

            var text = await cb.TryGetTextAsync();
            if (text != null)
                textSetter.Invoke(text);
        }

        internal async Task CopyTextFromClipboard(Func<string, Task> textSetter)
        {
            var cb = top.Clipboard;
            if (cb == null)
                return;

            var text = await cb.TryGetTextAsync();
            if (text != null)
                await textSetter.Invoke(text);
        }

        internal async Task CopyTextToClipboard(Func<string?> textGetter)
        {
            var cb = top.Clipboard;
            if (cb == null)
                return;

            var text = textGetter();
            if (text == null)
                return;

            await cb.SetTextAsync(text);
        }

        internal async Task CopyTextToClipboard(Func<Task<string?>> textGetter)
        {
            var cb = top.Clipboard;
            if (cb == null)
                return;

            var text = await textGetter();
            if (text == null)
                return;

            await cb.SetTextAsync(text);
        }
    }

    internal static TopLevel? GetActualTopLevel(this Visual? visual)
    {
        var top = TopLevel.GetTopLevel(visual);

        if (top is PopupRoot)
        {
            top = null;
        }

        if (top != null)
            return top;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            return TopLevel.GetTopLevel(single.MainView);
        }

        return null;
    }

    private static async Task ProcessSingleFolderPickAsync<T>(
        IStorageFolder folder,
        Func<T, Task> folderPickAsyncAction
    )
    {
        if (typeof(T) == typeof(IStorageFolder))
        {
            var exact = Unsafe.As<IStorageFolder, T>(ref folder);
            await folderPickAsyncAction.Invoke(exact);
            return;
        }

        var path = folder.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
            return;

        var result = ConvertPath<T>(path, typeof(IProgress<string>));

        await folderPickAsyncAction.Invoke(result);
    }

    private static async Task ProcessSingleFilePickAsync<T>(IStorageFile file, Func<T, Task> filePickAsyncAction)
    {
        if (typeof(T) == typeof(IStorageFile))
        {
            var exact = Unsafe.As<IStorageFile, T>(ref file);
            await filePickAsyncAction.Invoke(exact);
            return;
        }

        var path = file.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
            return;

        var result = ConvertPath<T>(path, typeof(DirectoryInfo));

        await filePickAsyncAction.Invoke(result);
    }

    private static T ConvertPath<T>(string path, params Type[] unsupportedTypes)
    {
        if (unsupportedTypes.Contains(typeof(T)))
            throw new NotSupportedException($"{typeof(T).Name}");

        if (typeof(T) == typeof(DirectoryInfo))
        {
            var d = new DirectoryInfo(path);
            return Unsafe.As<DirectoryInfo, T>(ref d);
        }

        if (typeof(T) == typeof(DirectoryInfo))
        {
            var f = new DirectoryInfo(path);
            return Unsafe.As<DirectoryInfo, T>(ref f);
        }

        if (typeof(T) == typeof(Uri))
        {
            var uri = new Uri(path);
            return Unsafe.As<Uri, T>(ref uri);
        }

        if (typeof(T) == typeof(string))
        {
            return Unsafe.As<string, T>(ref path);
        }

        throw new NotSupportedException($"{typeof(T).Name}");
    }
}
