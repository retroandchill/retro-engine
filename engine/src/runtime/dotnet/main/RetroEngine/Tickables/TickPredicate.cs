// @file TickPredicate.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

internal sealed class TickPredicate : ITickable
{
    private readonly TaskCompletionSource _tcs = new();
    private readonly Func<bool> _predicate;
    private readonly CancellationToken _cancellationToken;

    public TickGroup TickGroup => TickGroup.Simulation;
    public bool TickEnabled => !_cancellationToken.IsCancellationRequested && !_tcs.Task.IsCompleted;

    public Task Task => _tcs.Task;

    public TickPredicate(Func<bool> predicate, CancellationToken cancellationToken = default)
    {
        _predicate = predicate;
        _cancellationToken = cancellationToken;
        _cancellationToken.Register(() => _tcs.TrySetCanceled(_cancellationToken));
    }

    public void Tick(float deltaTime)
    {
        if (_predicate())
        {
            _tcs.TrySetResult();
        }
    }
}
