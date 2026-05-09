// // @file AssetCreationService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Assets;
using RetroEngine.Editor.Core.Services.Factories;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class AssetCreationService(
    AssetManager assetManager,
    IEnumerable<IAssetFactory> factories,
    IEnumerable<IAssetEncoder> encoders
)
{
    private readonly ImmutableDictionary<Name, IAssetFactory> _factories = factories.ToImmutableDictionary(f =>
        f.AssetType
    );
    private readonly ImmutableDictionary<Name, IAssetEncoder> _encoders = encoders.ToImmutableDictionary(e =>
        e.AssetType
    );

    public async ValueTask<object> CreateAssetAsync(
        Name assetPackage,
        Name parentEntry,
        Name assetType,
        string desiredName,
        CancellationToken cancellationToken = default
    )
    {
        if (assetManager.GetPackage(assetPackage) is not IEditableAssetPackage package)
            throw new AssetLoadException($"Package '{assetPackage}' not found");

        var encoder = _encoders.GetValueOrDefault(assetType);
        if (encoder is null)
            throw new AssetLoadException($"No encoder found for asset type '{assetType}'");

        var extensionIndex = desiredName.LastIndexOf('.');
        var targetName = desiredName;
        ReadOnlySpan<char> targetExtension = encoder.DefaultExtension;
        var hasValidExtension = false;
        if (extensionIndex >= 0)
        {
            var extension = desiredName.AsSpan(extensionIndex + 1);
            foreach (var validExtension in encoder.Extensions)
            {
                if (!validExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    continue;
                targetExtension = validExtension;
                hasValidExtension = true;
                break;
            }
        }

        if (!hasValidExtension)
        {
            targetName = $"{desiredName}.{targetExtension}";
        }

        AssetPath desiredPath;
        if (parentEntry.IsNone)
        {
            desiredPath = new AssetPath(assetPackage, targetName);
        }
        else
        {
            var entry = package.GetEntry(parentEntry);
            if (entry is null)
                throw new AssetLoadException($"Entry '{parentEntry}' not found in package '{assetPackage}'");

            desiredPath = new AssetPath(assetPackage, $"{parentEntry.ToString()}/{targetName}");
        }

        var factory = _factories.GetValueOrDefault(assetPackage);
        if (factory is null)
            throw new AssetLoadException($"No factory found for asset type '{assetPackage}'");

        var asset = factory.CreateAsset(desiredPath);

        await package.AddAssetAsync(desiredPath.AssetName, assetType, asset, cancellationToken);
        return asset;
    }
}
