// // @file AssetToolsImpl.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.ComponentModel;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using RetroEngine.Assets;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;
using ZLinq;

namespace RetroEngine.AssetTools;

[RegisterSingleton<IAssetTools>]
internal partial class AssetToolsImpl : IAssetTools
{
    private readonly ImmutableDictionary<Type, IAssetFactory> _factories;
    private readonly ImmutableDictionary<Type, IAssetEncoder> _encoders;
    private readonly ImmutableArray<IAssetTypeActions> _assetTypeActions;
    private readonly ImmutableDictionary<Type, int> _assetTypeActionsLookup;

    private readonly Dictionary<Name, AdvancedAssetCategory> _allocatedCategoryBits = new();
    private readonly Dictionary<AssetTypeCategories, Text> _categoryDisplayNames = new();

    private uint _nextUserCategoryBit;
    private readonly AssetManager _assetManager;
    private readonly ILogger<AssetToolsImpl> _logger;

    public AssetToolsImpl(
        AssetManager assetManager,
        IEnumerable<IAssetFactory> factories,
        IEnumerable<IAssetEncoder> encoders,
        IEnumerable<AssetToolCustomizer> customizers,
        IEnumerable<IAssetTypeActions> assetTypeActions,
        ILogger<AssetToolsImpl> logger
    )
    {
        _assetManager = assetManager;
        _logger = logger;
        _factories = factories.ToImmutableDictionary(f => f.AssetType);
        _encoders = encoders.ToImmutableDictionary(e => e.AssetType);

        _assetTypeActions = [.. assetTypeActions];
        var builder = ImmutableDictionary.CreateBuilder<Type, int>();
        foreach (var (i, action) in _assetTypeActions.AsValueEnumerable().Index())
        {
            builder.Add(action.SupportedType, i);
        }
        _assetTypeActionsLookup = builder.ToImmutable();

        TextKey assetCategoryNamespace = "AssetCategories";
        AddAdvancedAssetCategory(
            AssetTypeCategories.Audio,
            nameof(AssetTypeCategories.Audio),
            Text.AsLocalizable(assetCategoryNamespace, "Audio", "Audio")
        );
        AddAdvancedAssetCategory(
            AssetTypeCategories.Data,
            nameof(AssetTypeCategories.Data),
            Text.AsLocalizable(assetCategoryNamespace, "Data", "Data")
        );
        AddAdvancedAssetCategory(
            AssetTypeCategories.Graphics,
            nameof(AssetTypeCategories.Graphics),
            Text.AsLocalizable(assetCategoryNamespace, "Graphics", "Graphics")
        );
        AddAdvancedAssetCategory(
            AssetTypeCategories.Gameplay,
            nameof(AssetTypeCategories.Gameplay),
            Text.AsLocalizable(assetCategoryNamespace, "Gameplay", "Gameplay")
        );
        AddAdvancedAssetCategory(
            AssetTypeCategories.Scripting,
            nameof(AssetTypeCategories.Scripting),
            Text.AsLocalizable(assetCategoryNamespace, "Scripting", "Scripting")
        );
        AddAdvancedAssetCategory(
            AssetTypeCategories.UI,
            nameof(AssetTypeCategories.UI),
            Text.AsLocalizable(assetCategoryNamespace, "UI", "UI")
        );
        AddAdvancedAssetCategory(
            AssetTypeCategories.Misc,
            nameof(AssetTypeCategories.Misc),
            Text.AsLocalizable(assetCategoryNamespace, "Misc", "Misc")
        );

        foreach (var customizer in customizers)
        {
            customizer.Configure(this);
        }
    }

    private void AddAdvancedAssetCategory(AssetTypeCategories category, Name categoryKey, Text categoryDisplayName)
    {
        _allocatedCategoryBits.Add(categoryKey, new AdvancedAssetCategory(category, categoryKey, categoryDisplayName));
        _categoryDisplayNames[category] = categoryDisplayName;
    }

    public ImmutableArray<IAssetTypeActions> AssetTypeActions => _assetTypeActions;

    public IAssetTypeActions? FindAssetTypeAction(Type type)
    {
        return _assetTypeActionsLookup.TryGetValue(type, out var index) ? _assetTypeActions[index] : null;
    }

    public AssetTypeCategories RegisterAdvancedAssetCategory(Name categoryKey, Text categoryDisplayName)
    {
        var result = FindAdvancedAssetCategory(categoryKey);
        if (result != AssetTypeCategories.Misc)
            return result;

        if (_nextUserCategoryBit == 0)
        {
            LogAssetBitsExhausted(categoryKey, categoryDisplayName);
            return result;
        }

        result = (AssetTypeCategories)_nextUserCategoryBit;
        _allocatedCategoryBits.Add(categoryKey, new AdvancedAssetCategory(result, categoryKey, categoryDisplayName));
        _categoryDisplayNames[result] = categoryDisplayName;

        LogBitTaken(categoryKey, categoryDisplayName, _nextUserCategoryBit, (int)Math.Floor(Math.Log2((int)result)));
        if (_nextUserCategoryBit == (uint)AssetTypeCategories.LastUser)
        {
            _nextUserCategoryBit = 0;
        }
        else
        {
            _nextUserCategoryBit <<= 1;
        }

        return result;
    }

    public AssetTypeCategories FindAdvancedAssetCategory(Name categoryKey)
    {
        return _allocatedCategoryBits.TryGetValue(categoryKey, out var result)
            ? result.Category
            : AssetTypeCategories.Misc;
    }

    public IEnumerable<AdvancedAssetCategory> AdvancedAssetCategories => _allocatedCategoryBits.Values;

    public IEnumerable<IAssetFactory> Factories => _factories.Values;

    public string? GetDefaultAssetExtension(Type assetType)
    {
        return _encoders.GetValueOrDefault(assetType)?.DefaultExtension;
    }

    public Text GetAssetCategoryDisplayName(AssetTypeCategories category)
    {
        switch (category)
        {
            case AssetTypeCategories.None:
                return Text.Empty;
            case AssetTypeCategories.Audio:
                return KnownAssetTypes.Audio;
            case AssetTypeCategories.Data:
                return KnownAssetTypes.Data;
            case AssetTypeCategories.Graphics:
                return KnownAssetTypes.Graphics;
            case AssetTypeCategories.Gameplay:
                return KnownAssetTypes.Gameplay;
            case AssetTypeCategories.Scripting:
                return KnownAssetTypes.Scripting;
            case AssetTypeCategories.UI:
                return KnownAssetTypes.UI;
            case AssetTypeCategories.Misc:
                return KnownAssetTypes.Misc;
            case AssetTypeCategories.FirstUser:
            case AssetTypeCategories.LastUser:
            default:
                return _categoryDisplayNames.GetValueOrDefault(category, KnownAssetTypes.Misc);
        }
    }

    public async Task<object> CreateAssetAsync(
        ReadOnlyMemory<char> assetName,
        ReadOnlyMemory<char> parentPath,
        Name assetPackage,
        Type assetType,
        CancellationToken cancellationToken = default
    )
    {
        var targetName = GetAssetNameWithExtension(assetName.Span, assetType);
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

    public ReadOnlySpan<char> GetAssetNameWithExtension(ReadOnlySpan<char> assetName, Type assetType)
    {
        var encoder = _encoders.GetValueOrDefault(assetType);
        if (encoder is null)
            throw new AssetLoadException($"No encoder found for asset type '{assetType}'");

        var extensionIndex = assetName.LastIndexOf('.');
        ReadOnlySpan<char> targetExtension = encoder.DefaultExtension;
        var hasValidExtension = false;
        if (extensionIndex >= 0)
        {
            var extension = assetName[(extensionIndex + 1)..];
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
            return $"{assetName}.{targetExtension}";
        }

        return assetName;
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
