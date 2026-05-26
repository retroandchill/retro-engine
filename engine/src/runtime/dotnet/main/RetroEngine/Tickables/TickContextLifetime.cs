// @file TickContextLifetime.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
internal sealed class TickContextLifetime(TickManager tickManager) : IEngineContextLifetime
{
    public bool Initialized { get; private set; }

    public void Initialize()
    {
        if (Initialized)
            throw new InvalidOperationException("TickContextLifetime is already initialized.");

        TickManager.Instance = tickManager;
        Initialized = true;
    }

    public void Shutdown()
    {
        if (!Initialized)
            throw new InvalidOperationException("TickContextLifetime is not initialized.");

        TickManager.Instance = null;
        Initialized = false;
    }
}
