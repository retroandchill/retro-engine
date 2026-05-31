// @file InputContextLifetime.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Interaction;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
internal sealed class InputContextLifetime(InputManager manager) : IEngineContextLifetime
{
    public bool Initialized { get; private set; }

    public void Initialize()
    {
        if (Initialized)
            throw new InvalidOperationException("InputContextLifetime is already initialized.");

        Initialized = true;
        Input.Manager = manager;
    }

    public void Shutdown()
    {
        if (!Initialized)
            throw new InvalidOperationException("InputContextLifetime is not initialized.");

        Input.Manager = null!;
        Initialized = false;
    }
}
