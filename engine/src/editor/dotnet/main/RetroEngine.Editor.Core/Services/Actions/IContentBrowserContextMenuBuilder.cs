// // @file IContentBrowserContextMenuBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.Input;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.ToolMenu.ViewModel;

namespace RetroEngine.Editor.Core.Services.Actions;

public readonly record struct ContentBrowserCommands(
    IRelayCommand<ContentBrowserItem> NewFolderCommand,
    IRelayCommand<NewAssetArgs> NewAssetCommand,
    IRelayCommand<ContentBrowserItem> RefreshCommand,
    IRelayCommand<ContentBrowserItem> RenameCommand,
    IRelayCommand<ContentBrowserItem> DeleteCommand
);

public interface IContentBrowserContextMenuBuilder
{
    IReadOnlyList<IMenuItemEntry> Build(ContentBrowserItem? selectedItem, in ContentBrowserCommands commands);
}
