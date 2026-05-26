// @file AssetContextLifetime.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Assets;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
internal sealed class AssetContextLifetime(IAssetManager manager) : IEngineContextLifetime
{
    public bool Initialized { get; private set; }

    public void Initialize()
    {
        if (Initialized)
            throw new InvalidOperationException("AssetContextLifetime is already initialized.");

        Asset.Manager = manager;
        Initialized = true;
    }

    public void Shutdown()
    {
        if (!Initialized)
            throw new InvalidOperationException("AssetContextLifetime is not initialized.");

        Asset.Manager = null;
        Initialized = false;
    }
}
