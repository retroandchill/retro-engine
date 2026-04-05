// // @file AsyncSerialQueue.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities.Async;

public sealed class AsyncSerialQueue
{
    private readonly Lock _lock = new();
    private Task _tail = Task.CompletedTask;

    public Task Enqueue(Func<Task> work)
    {
        ArgumentNullException.ThrowIfNull(work);

        using var scope = _lock.EnterScope();
        _tail = _tail
            .ContinueWith(
                _ => work(),
                CancellationToken.None,
                TaskContinuationOptions.DenyChildAttach,
                TaskScheduler.Default
            )
            .Unwrap();

        return _tail;
    }

    public Task Enqueue(Action work)
    {
        return Enqueue(() =>
        {
            work();
            return Task.CompletedTask;
        });
    }

    public Task WhenIdle()
    {
        using var scope = _lock.EnterScope();
        return _tail;
    }
}
