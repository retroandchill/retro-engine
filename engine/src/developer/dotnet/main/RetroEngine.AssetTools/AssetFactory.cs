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
}

public interface IAssetFactory<out T> : IAssetFactory
    where T : class
{
    new T CreateAsset(AssetPath path);
}

public abstract class AssetFactory<T> : IAssetFactory<T>
    where T : class
{
    public Type AssetType => typeof(T);

    public abstract T CreateAsset(AssetPath path);

    object IAssetFactory.CreateAsset(AssetPath path) => CreateAsset(path);
}
