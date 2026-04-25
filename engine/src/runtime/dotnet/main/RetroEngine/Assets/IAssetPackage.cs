// // @file IAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public enum AssetPackageLoadState
{
    Unloaded,
    Loading,
    Loaded,
}

public interface IAssetPackage
{
    Name PackageName { get; }

    AssetPackageLoadState LoadState { get; }

    public string SourcePath { get; }

    public bool IsReadOnly { get; }

    public ValueTask LoadAsync(CancellationToken cancellationToken = default);

    public void Unload();

    bool HasAsset(Name assetName);

    Name GetAssetType(Name assetName);

    Stream OpenAsset(Name assetName);

    void Serialize<T, TBufferWriter>(in TBufferWriter writer, T? data)
        where TBufferWriter : IBufferWriter<byte>;

    T? Deserialize<T>(in ReadOnlySequence<byte> sequence);

    void Deserialize<T>(in ReadOnlySequence<byte> sequence, ref T? data)
    {
        data = Deserialize<T>(sequence);
    }
}

public interface IAssetPackageFactory
{
    bool CanCreate(Name packageName, string path);

    IAssetPackage Create(Name packageName, string path);
}
