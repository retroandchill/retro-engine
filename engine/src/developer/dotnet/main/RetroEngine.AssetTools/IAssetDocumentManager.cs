// // @file IAssetDocumentManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.AssetTools.ViewModels;

namespace RetroEngine.AssetTools;

public interface IAssetDocumentManager
{
    IEnumerable<IAssetViewModel> GetOpenDocuments();

    (IAssetViewModel Document, bool IsNew) OpenDocument(AssetPath assetPath, object asset);

    void CloseDocument(IAssetViewModel document);
}
