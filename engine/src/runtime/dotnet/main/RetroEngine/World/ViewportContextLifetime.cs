// @file ViewportContextLifetime.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.World;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
internal class ViewportContextLifetime(ViewportManager viewportManager) : IEngineContextLifetime
{
    public bool Initialized { get; private set; }

    public void Initialize()
    {
        if (Initialized)
            throw new InvalidOperationException("ViewportContextLifetime is already initialized.");
        Viewport.Manager = viewportManager;
        Initialized = true;
    }

    public void Shutdown()
    {
        if (!Initialized)
            throw new InvalidOperationException("ViewportContextLifetime is not initialized.");
        Viewport.Manager = null;
        Initialized = false;
    }
}
