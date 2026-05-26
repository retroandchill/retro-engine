// @file TickHandle.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

public sealed class TickHandle : IDisposable
{
    private readonly ITickable _tickable;
    private bool _disposed;

    public TickHandle(ITickable tickable)
    {
        if (TickManager.Instance is null)
            throw new InvalidOperationException("Tick manager is not initialized.");

        _tickable = tickable;
        TickManager.Instance.RegisterTickable(tickable);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        TickManager.Instance?.UnregisterTickable(_tickable);
        _disposed = true;
    }
}
