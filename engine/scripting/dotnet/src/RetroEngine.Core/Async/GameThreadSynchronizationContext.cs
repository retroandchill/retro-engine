// // @file GameThreadSynchronizationContext.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace RetroEngine.Core.Async;

public sealed class GameThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly int _gameThreadId = Environment.CurrentManagedThreadId;
    private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _workItems = new();

    public bool IsOnGameThread => _gameThreadId == Environment.CurrentManagedThreadId;

    public event Action<Exception>? UnhandledException;

    public override void Post(SendOrPostCallback d, object? state)
    {
        ArgumentNullException.ThrowIfNull(d);
        _workItems.Enqueue((d, state));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        throw new NotSupportedException(
            "Synchronous task waiting will almost always result in a deadlock, so it is not allowed."
        );
    }

    public int Pump(int maxWorkItems = int.MaxValue)
    {
        if (!IsOnGameThread)
        {
            throw new InvalidOperationException("Can only pump work items on the game thread.");
        }

        if (maxWorkItems <= 0)
            return 0;

        var processed = 0;

        while (processed < maxWorkItems)
        {
            if (!_workItems.TryDequeue(out var workItem))
                break;

            try
            {
                workItem.Callback(workItem.State);
            }
            catch (Exception ex)
            {
                UnhandledException?.Invoke(ex);
            }

            processed++;
        }

        return processed;
    }

    public void Dispose()
    {
        _workItems.Clear();
    }
}
