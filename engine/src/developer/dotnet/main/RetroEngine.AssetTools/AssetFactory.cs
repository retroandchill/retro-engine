// // @file AssetFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;

namespace RetroEngine.AssetTools;

public interface IAssetFactory
{
    Type AssetType { get; }

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

    public abstract T CreateAsset(AssetPath path);

    public virtual ValueTask<T> CreateAssetAsync(AssetPath path, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(CreateAsset(path));
    }

    object IAssetFactory.CreateAsset(AssetPath path) => CreateAsset(path);

    async ValueTask<object> IAssetFactory.CreateAssetAsync(AssetPath path, CancellationToken cancellationToken)
    {
        return await CreateAssetAsync(path, cancellationToken);
    }
}
