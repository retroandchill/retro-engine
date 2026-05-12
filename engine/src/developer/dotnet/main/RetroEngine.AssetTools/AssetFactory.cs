// // @file AssetFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using CaseConverter;
using Namotion.Reflection;
using RetroEngine.Assets;
using RetroEngine.Localization.Reflection;
using RetroEngine.Portable.Localization;
using RetroEngine.Utilities;

namespace RetroEngine.AssetTools;

public interface IAssetFactory
{
    Type AssetType { get; }

    Text DisplayName { get; }

    Text ToolTip { get; }

    AssetTypeCategories Categories { get; }

    object CreateAsset(AssetPath path);

    ValueTask<object> CreateAssetAsync(AssetPath path, CancellationToken cancellationToken = default);
}

public interface IAssetFactory<T> : IAssetFactory
    where T : class
{
    new T CreateAsset(AssetPath path);

    new ValueTask<T> CreateAssetAsync(AssetPath path, CancellationToken cancellationToken = default);
}

public abstract class AssetFactory<T> : IAssetFactory<T>
    where T : class
{
    public Type AssetType => typeof(T);

    public virtual Text DisplayName => LocalizedType.Get<T>().DisplayName;

    public virtual Text ToolTip => LocalizedType.Get<T>().Description;

    public virtual AssetTypeCategories Categories => AssetTypeCategories.Misc;

    public abstract T CreateAsset(AssetPath path);

    public virtual ValueTask<T> CreateAssetAsync(AssetPath path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(CreateAsset(path));
    }

    object IAssetFactory.CreateAsset(AssetPath path) => CreateAsset(path);

    async ValueTask<object> IAssetFactory.CreateAssetAsync(AssetPath path, CancellationToken cancellationToken)
    {
        return await CreateAssetAsync(path, cancellationToken);
    }
}
