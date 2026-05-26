// @file TickAwait.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

internal sealed class TickAwait : ITickable
{
    private readonly TaskCompletionSource _tcs = new();
    private readonly ulong _startedOn;
    private readonly ulong _duration;
    private readonly CancellationToken _cancellationToken;

    public TickAwait(ulong duration, CancellationToken cancellationToken = default)
    {
        if (TickManager.Instance is null)
            throw new InvalidOperationException("Tick manager is not initialized.");

        _startedOn = TickManager.Instance.FrameCount;
        _duration = duration;
        _cancellationToken = cancellationToken;
    }

    public TickGroup TickGroup => TickGroup.Simulation;

    public bool TickEnabled => !_cancellationToken.IsCancellationRequested || _tcs.Task.IsCompleted;

    public Task Task => _tcs.Task;

    public void Tick(float deltaTime)
    {
        if (_tcs.Task.IsCompleted)
            return;

        if (_cancellationToken.IsCancellationRequested)
        {
            _tcs.SetCanceled(_cancellationToken);
            return;
        }

        if (TickManager.Instance!.FrameCount - _startedOn >= _duration)
            _tcs.SetResult();
    }
}
