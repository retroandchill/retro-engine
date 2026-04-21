// // @file TickHandle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

public sealed class TickHandle : IDisposable
{
    private readonly ITickable _tickable;
    private readonly TickManager _tickManager;
    private bool _disposed;

    public TickHandle(ITickable tickable, TickManager tickManager)
    {
        _tickable = tickable;
        _tickManager = tickManager;
        _tickManager.RegisterTickable(tickable);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _tickManager.UnregisterTickable(_tickable);
        _disposed = true;
    }
}
