// // @file AssetDocumentManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class AssetDocumentManager(AssetViewModelProvider viewModelProvider) : IAssetDocumentManager
{
    private readonly Dictionary<AssetPath, IAssetViewModel> _openDocuments = new();

    public IEnumerable<IAssetViewModel> GetOpenDocuments()
    {
        return _openDocuments.Values;
    }

    public async ValueTask<(IAssetViewModel Document, bool IsNew)> OpenDocumentAsync(
        AssetPath assetPath,
        CancellationToken cancellationToken = default
    )
    {
        if (_openDocuments.TryGetValue(assetPath, out var document))
            return (document, false);

        document = await viewModelProvider.GetViewModelAsync(assetPath, cancellationToken);
        _openDocuments.Add(assetPath, document);
        return (document, true);
    }

    public void CloseDocument(IAssetViewModel document)
    {
        _openDocuments.Remove(document.Path);
    }
}
