// @file IEngineContextLifetime.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;

namespace RetroEngine;

public interface IEngineContextLifetime
{
    int Priority => 0;
    bool Initialized { get; }

    void Initialize();

    void Shutdown();
}

public static class EngineContextLifetimeExtensions
{
    public static IDisposable CreateLifetimeScope(this IEngineContextLifetime contextLifetime)
    {
        if (contextLifetime.Initialized)
            throw new InvalidOperationException("Context lifetime has already been initialized.");

        contextLifetime.Initialize();
        return Disposable.Create(contextLifetime, l => l.Shutdown());
    }
}
