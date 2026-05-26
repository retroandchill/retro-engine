// @file SceneContextLifetime.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.World;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
internal class SceneContextLifetime(SceneManager sceneManager, ViewportManager viewportManager) : IEngineContextLifetime
{
    public bool Initialized { get; private set; }

    public void Initialize()
    {
        Scene.Manager = sceneManager;
        Viewport.Manager = viewportManager;
        Initialized = true;
    }

    public void Shutdown()
    {
        Scene.Manager = null;
        Viewport.Manager = null;
        Initialized = false;
    }
}
