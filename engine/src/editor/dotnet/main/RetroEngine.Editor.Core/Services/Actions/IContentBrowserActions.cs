// // @file IContentBrowserActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Services.Actions;

public interface IContentBrowserActions
{
    Task NewAssetAsync(
        ContentBrowserItem parent,
        Type assetType,
        Text displayName,
        Action<AssetPath, object>? postCreationAction = null
    );

    Task NewFolderAsync(ContentBrowserItem parent, IEditableAssetPackage editablePackage);
}
