// // @file Asset.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets;

public abstract partial class Asset(AssetPath path) : IDisposable
{
    public AssetPath Path { get; } = path;

    protected bool Disposed { get; private set; }

    ~Asset()
    {
        Dispose(false);
    }

    [CreateSyncVersion]
    public static ValueTask<Asset?> LoadAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        return Engine.Instance.LoadAssetAsync(path, cancellationToken);
    }

    [CreateSyncVersion]
    public static async ValueTask<T?> LoadAsync<T>(AssetPath path, CancellationToken cancellationToken = default)
        where T : Asset
    {
        return await LoadAsync(path, cancellationToken) as T;
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        Dispose(true);
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }
}
