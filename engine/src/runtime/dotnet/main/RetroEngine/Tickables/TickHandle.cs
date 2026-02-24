// // @file TickHandle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

public sealed class TickHandle : IDisposable
{
    private readonly ITickable _tickable;
    private bool _disposed;

    public TickHandle(ITickable tickable)
    {
        _tickable = tickable;
        Engine.Instance.RegisterTickable(tickable);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Engine.Instance.UnregisterTickable(_tickable);
        _disposed = true;
    }
}
