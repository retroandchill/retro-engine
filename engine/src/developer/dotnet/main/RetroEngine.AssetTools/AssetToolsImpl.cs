// // @file AssetToolsImpl.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using CaseConverter;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using RetroEngine.Assets;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.AssetTools;

[RegisterSingleton<IAssetTools>]
internal partial class AssetToolsImpl : IAssetTools
{
    private readonly ImmutableDictionary<Type, IAssetEncoder> _encoders;
    private readonly ConcurrentDictionary<Type, AssetTypeCategories> _assetTypeAssociations = new();
    private readonly List<IAssetTypeActions> _assetTypeActions = [];
    private readonly Dictionary<Type, int> _assetTypeActionsLookup = new();

    private readonly Dictionary<Name, AdvancedAssetCategory> _allocatedCategoryBits = new();
    private readonly Dictionary<AssetTypeCategories, Text> _categoryDisplayNames = new();

    private uint _nextUserCategoryBit;
    private readonly AssetManager _assetManager;
    private readonly ILogger<AssetToolsImpl> _logger;

    public AssetToolsImpl(
        AssetManager assetManager,
        IEnumerable<IAssetEncoder> encoders,
        IEnumerable<AssetToolCustomizer> customizers,
        ILogger<AssetToolsImpl> logger
    )
    {
        _assetManager = assetManager;
        _logger = logger;
        _encoders = encoders.ToImmutableDictionary(e => e.AssetType);

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

    public void RegisterAssetTypeActions(IAssetTypeActions actions)
    {
        RemoveAssetTypeActionsBySupportedType(actions.SupportedType);
        var index = _assetTypeActions.Count;
        _assetTypeActionsLookup[actions.SupportedType] = index;
        _assetTypeActions.Add(actions);
    }

    public void UnregisterAssetTypeActions(IAssetTypeActions actions)
    {
        RemoveAssetTypeActionsBySupportedType(actions.SupportedType);
    }

    private void RemoveAssetTypeActionsBySupportedType(Type supportedType)
    {
        if (!_assetTypeActionsLookup.Remove(supportedType, out var index))
            return;

        var lastIndex = _assetTypeActions.Count - 1;
        if (lastIndex != index)
        {
            var last = _assetTypeActions[^1];
            _assetTypeActionsLookup[last.SupportedType] = index;
            _assetTypeActions[index] = last;
        }

        _assetTypeActions.RemoveAt(lastIndex);
    }

    public IReadOnlyList<IAssetTypeActions> AssetTypeActions => _assetTypeActions;

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
        IAssetFactory factory,
        CancellationToken cancellationToken = default
    )
    {
        var assetType = factory.AssetType;
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
