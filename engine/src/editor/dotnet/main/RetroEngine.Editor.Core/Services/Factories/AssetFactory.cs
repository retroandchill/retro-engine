// // @file AssetFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.Services.Factories;

public interface IAssetFactory
{
    Name AssetType { get; }
    object CreateAsset(AssetPath path);
}

public interface IAssetFactory<out T> : IAssetFactory
    where T : class
{
    new T CreateAsset(AssetPath path);
}

public abstract class AssetFactory<T>(Name assetType) : IAssetFactory<T>
    where T : class
{
    public Name AssetType { get; } = assetType;

    public abstract T CreateAsset(AssetPath path);

    object IAssetFactory.CreateAsset(AssetPath path) => CreateAsset(path);
}
