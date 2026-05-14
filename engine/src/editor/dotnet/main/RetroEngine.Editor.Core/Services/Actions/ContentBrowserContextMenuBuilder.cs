// // @file ContentBrowserContextMenuBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.AssetTools;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.ToolMenu.ViewModel;

namespace RetroEngine.Editor.Core.Services.Actions;

[RegisterSingleton]
public sealed class ContentBrowserContextMenuBuilder(IAssetTools assetTools) : IContentBrowserContextMenuBuilder
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Tabs.ContentBrowserContextMenuBuilder";

    private static readonly Text CreateLabel = Text.AsLocalizable(TextNamespace, "Create", "Create");
    private static readonly Text NewFolderLabel = Text.AsLocalizable(TextNamespace, "NewFolder", "New Folder");
    private static readonly Text CommonLabel = Text.AsLocalizable(TextNamespace, "Common", "Common");
    private static readonly Text RefreshLabel = Text.AsLocalizable(TextNamespace, "Refresh", "Refresh");
    private static readonly Text RenameLabel = Text.AsLocalizable(TextNamespace, "Rename", "Rename");
    private static readonly Text DeleteLabel = Text.AsLocalizable(TextNamespace, "DeleteLabel", "Delete");

    public IReadOnlyList<IMenuItemEntry> Build(ContentBrowserItem? selectedItem, in ContentBrowserCommands commands)
    {
        if (selectedItem is null)
        {
            return [];
        }

        var newContextActions = new List<IMenuItemEntry>();
        if (selectedItem.IsDirectory)
        {
            newContextActions.AddRange(
                new MenuSectionHeader("Create", CreateLabel),
                new MenuCommand("NewFolder", NewFolderLabel, commands.NewFolderCommand)
                {
                    IsEnabled = selectedItem.CanEdit,
                }
            );

            var newAssetCommand = commands.NewAssetCommand;
            var sectionsToAdd = assetTools
                .AdvancedAssetCategories.OrderBy(x => x.CategoryName)
                .Select(x =>
                {
                    var factories = assetTools.Factories.Where(f => f.Categories.HasFlag(x.Category)).ToArray();

                    return (Category: x, Factories: factories);
                })
                .Where(x => x.Factories.Length > 0)
                .Select(x =>
                {
                    var subMenu = new SubMenu(x.Category.CategoryKey, x.Category.CategoryName);

                    subMenu.AddRange(
                        x.Factories.Select(f => new ParameterizedMenuCommand(
                            f.AssetType.Name,
                            f.DisplayName,
                            newAssetCommand,
                            new NewAssetArgs(selectedItem, f.AssetType, f.DisplayName)
                        ))
                    );
                    return subMenu;
                })
                .ToArray();
            if (sectionsToAdd.Length > 0)
            {
                newContextActions.Add(IMenuSeparator.Instance);
                newContextActions.AddRange(sectionsToAdd);
            }
        }

        newContextActions.AddRange(
            new MenuSectionHeader("Common", CommonLabel),
            new ParameterizedMenuCommand("Refresh", RefreshLabel, commands.RefreshCommand, selectedItem),
            new ParameterizedMenuCommand("Rename", RenameLabel, commands.RenameCommand, selectedItem)
            {
                IsEnabled = selectedItem.CanRenameOrDelete,
            },
            new ParameterizedMenuCommand("Delete", DeleteLabel, commands.DeleteCommand, selectedItem)
            {
                IsEnabled = selectedItem.CanRenameOrDelete,
            }
        );

        return newContextActions;
    }
}
