// // @file AssetToolsImpl.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using RetroEngine.Assets;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.AssetTools;

[RegisterSingleton<IAssetTools>]
internal partial class AssetToolsImpl : IAssetTools
{
    private readonly ImmutableDictionary<Type, IAssetFactory> _factories;
    private readonly ImmutableDictionary<Type, IAssetEncoder> _encoders;
    private readonly Dictionary<Type, AssetTypeCategory> _assetTypeAssociations = new();

    private readonly Dictionary<Name, AdvancedAssetCategory> _allocatedCategoryBits = new();

    private uint _nextUserCategoryBit;
    private readonly AssetManager _assetManager;
    private readonly ILogger<AssetToolsImpl> _logger;

    public AssetToolsImpl(
        AssetManager assetManager,
        IEnumerable<IAssetFactory> factories,
        IEnumerable<IAssetEncoder> encoders,
        IEnumerable<AssetToolCustomizer> customizers,
        ILogger<AssetToolsImpl> logger
    )
    {
        _assetManager = assetManager;
        _logger = logger;
        _factories = factories.ToImmutableDictionary(f => f.AssetType);
        _encoders = encoders.ToImmutableDictionary(e => e.AssetType);
        foreach (var customizer in customizers)
        {
            customizer.Configure(this);
        }
    }

    public AssetTypeCategory RegisterAdvancedAssetCategory(Name categoryKey, Text categoryDisplayName)
    {
        var result = FindAdvancedAssetCategory(categoryKey);
        if (result != AssetTypeCategory.Misc)
            return result;

        if (_nextUserCategoryBit == 0)
        {
            LogAssetBitsExhausted(categoryKey, categoryDisplayName);
            return result;
        }

        result = unchecked((AssetTypeCategory)_nextUserCategoryBit);
        _allocatedCategoryBits.Add(categoryKey, new AdvancedAssetCategory(result, categoryDisplayName));

        LogBitTaken(categoryKey, categoryDisplayName, _nextUserCategoryBit, (int)Math.Floor(Math.Log2((int)result)));
        if (_nextUserCategoryBit == unchecked((uint)AssetTypeCategory.LastUser))
        {
            _nextUserCategoryBit = 0;
        }
        else
        {
            _nextUserCategoryBit <<= 1;
        }

        return result;
    }

    public AssetTypeCategory FindAdvancedAssetCategory(Name categoryKey)
    {
        return _allocatedCategoryBits.TryGetValue(categoryKey, out var result)
            ? result.Category
            : AssetTypeCategory.Misc;
    }

    public IEnumerable<AdvancedAssetCategory> AdvancedAssetCategories => _allocatedCategoryBits.Values;

    public void AssociateAssetType(Type assetType, AssetTypeCategory category)
    {
        _assetTypeAssociations[assetType] = category;
    }

    public async Task<object> CreateAssetAsync(
        ReadOnlyMemory<char> assetName,
        ReadOnlyMemory<char> parentPath,
        Name assetPackage,
        Type assetType,
        CancellationToken cancellationToken = default
    )
    {
        var encoder = _encoders.GetValueOrDefault(assetType);
        if (encoder is null)
            throw new AssetLoadException($"No encoder found for asset type '{assetType}'");

        var extensionIndex = assetName.Span.LastIndexOf('.');
        var targetName = assetName.Span;
        ReadOnlySpan<char> targetExtension = encoder.DefaultExtension;
        var hasValidExtension = false;
        if (extensionIndex >= 0)
        {
            var extension = assetName.Span[(extensionIndex + 1)..];
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
            targetName = $"{assetName}.{targetExtension}";
        }

        var desiredPath = parentPath.IsEmpty
            ? new AssetPath(assetPackage, new Name(targetName))
            : new AssetPath(assetPackage, new Name($"{parentPath.Span}/{targetName}"));

        var factory = _factories.GetValueOrDefault(assetType);
        if (factory is null)
            throw new AssetLoadException($"No factory found for asset type '{assetType}'");

        var asset = await factory.CreateAssetAsync(desiredPath, cancellationToken);
        await _assetManager.CreateAssetAsync(desiredPath, asset, cancellationToken);
        return asset;
    }

    [LoggerMessage(
        LogLevel.Warning,
        $"{nameof(RegisterAdvancedAssetCategory)}(\"{{Category}}\", \"{{displayName}}\") failed as all user "
            + "bits have been exhausted (placing into the Misc category instead)"
    )]
    private partial void LogAssetBitsExhausted(Name category, Text displayName);

    [LoggerMessage(
        LogLevel.Warning,
        $"{nameof(RegisterAdvancedAssetCategory)}(\"{{Category}}\", \"{{displayName}}\") used up bit "
            + "0x{Bit:X16}, Offset: {Offset}"
    )]
    private partial void LogBitTaken(Name category, Text displayName, uint bit, int offset);
}
