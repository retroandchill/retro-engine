// // @file Asset.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Assets;

public abstract class Asset(AssetPath path) : IDisposable
{
    public AssetPath Path { get; } = path;

    protected bool Disposed { get; private set; }

    ~Asset()
    {
        Dispose(false);
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
