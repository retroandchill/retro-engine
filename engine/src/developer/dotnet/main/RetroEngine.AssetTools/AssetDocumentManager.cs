// @file AssetDocumentManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Injectio.Attributes;
using RetroEngine.Assets;
using RetroEngine.AssetTools.Editing;
using RetroEngine.AssetTools.ViewModels;

namespace RetroEngine.AssetTools;

[RegisterSingleton]
public sealed class AssetDocumentManager(
    IAssetTools assetTools,
    IAssetManager assetManager,
    EditableAssetSessionManager sessionManager
) : IAssetDocumentManager
{
    private readonly Dictionary<AssetPath, IAssetViewModel> _openDocuments = new();

    public IEnumerable<IAssetViewModel> GetOpenDocuments()
    {
        return _openDocuments.Values;
    }

    public (IAssetViewModel Document, bool IsNew) OpenDocument(AssetPath assetPath, object asset)
    {
        if (_openDocuments.TryGetValue(assetPath, out var document))
            return (document, false);

        var assetTypeActions = GetAllParentTypes(asset.GetType())
            .Select(assetTools.FindAssetTypeAction)
            .FirstOrDefault(x => x is not null);

        if (assetTypeActions is not null)
        {
            document = assetTypeActions.CreateViewModel(assetPath, asset);
        }
        else
        {
            document = new GenericAssetViewModel(assetPath, asset);
        }

        var nameAsString = assetPath.AssetName.ToString();
        var lastDelimiter = nameAsString.LastIndexOf('/');
        document.Title = lastDelimiter >= 0 ? nameAsString[(lastDelimiter + 1)..] : nameAsString;

        _openDocuments.Add(assetPath, document);

        if (
            document is IAssetEditorViewModel editor
            && assetManager.FindPackage(assetPath.PackageName) is IEditableAssetPackage editablePackage
        )
        {
            sessionManager.Add(
                new EditableAssetSession(
                    editor,
                    async (saveTarget, cancellationToken) =>
                    {
                        await editablePackage.SaveExistingAssetAsync(
                            assetPath.AssetName,
                            saveTarget,
                            cancellationToken
                        );
                    },
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromSeconds(1)
                )
            );
        }

        return (document, true);
    }

    public async Task CloseDocumentAsync(IAssetViewModel document)
    {
        _openDocuments.Remove(document.Path);

        if (sessionManager.Remove(document.Path, out var session))
        {
            await session.DisposeAsync();
        }
    }

    private static IEnumerable<Type> GetAllParentTypes(Type type)
    {
        if (type == typeof(object))
            yield break;

        yield return type;
        if (type.BaseType is not null)
            foreach (var parentType in GetAllParentTypes(type.BaseType))
                yield return parentType;

        foreach (var interfaceType in type.GetInterfaces())
        foreach (var parentType in GetAllParentTypes(interfaceType))
            yield return parentType;
    }
}
