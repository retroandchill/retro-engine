// // @file TickAwait.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

internal sealed class TickAwait(ulong duration, CancellationToken cancellationToken = default) : ITickable
{
    private readonly TaskCompletionSource _tcs = new();
    private readonly ulong _startedOn = Engine.Instance.FrameCount;

    public bool TickEnabled => !cancellationToken.IsCancellationRequested || _tcs.Task.IsCompleted;

    public Task Task => _tcs.Task;

    public void Tick(float deltaTime)
    {
        if (_tcs.Task.IsCompleted)
            return;

        if (cancellationToken.IsCancellationRequested)
        {
            _tcs.SetCanceled(cancellationToken);
            return;
        }

        if (Engine.Instance.FrameCount - _startedOn >= duration)
            _tcs.SetResult();
    }
}
