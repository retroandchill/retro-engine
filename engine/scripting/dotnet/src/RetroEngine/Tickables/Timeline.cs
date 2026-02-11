// // @file Timeline.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

internal sealed class Timeline(float duration, Action<float> onTick, CancellationToken cancellationToken) : ITickable
{
    private readonly TaskCompletionSource _tcs = new();
    private float _elapsedTime;

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

        _elapsedTime += deltaTime;
        onTick.Invoke(Math.Min(_elapsedTime / duration, 1f));
        if (_elapsedTime >= duration)
        {
            _tcs.SetResult();
        }
    }
}
