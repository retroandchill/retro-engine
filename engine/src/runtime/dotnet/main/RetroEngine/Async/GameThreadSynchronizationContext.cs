// @file GameThreadSynchronizationContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using RetroEngine.Utilities.Async;

namespace RetroEngine.Async;

public sealed class GameThreadSynchronizationContext : SynchronizationContext, IThreadSync, IDisposable
{
    private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _workItems = new();

    public int SyncThreadId { get; private set; } = Environment.CurrentManagedThreadId;
    public bool IsOnGameThread => SyncThreadId == Environment.CurrentManagedThreadId;

    public event Action<Exception>? UnhandledException;

    public void AssignToGameThread()
    {
        SyncThreadId = Environment.CurrentManagedThreadId;
    }

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

    public void RunOnPrimaryThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (IsOnGameThread)
        {
            action();
            return;
        }

        Post(
            state =>
            {
                var act = (Action)state!;
                act();
            },
            action
        );
    }

    public void RunOnPrimaryThread(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsOnGameThread)
        {
            action()
                .ContinueWith(
                    t =>
                    {
                        if (t is { IsFaulted: true, Exception: { } ex })
                        {
                            RunOnPrimaryThread(() =>
                            {
                                var flatten = ex.Flatten();
                                UnhandledException?.Invoke(flatten);
                            });
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default
                );
            return;
        }

        // ReSharper disable once AsyncVoidLambda
        Post(
            static state =>
            {
                var (act, self) = ((Func<Task>, GameThreadSynchronizationContext))state!;
                act()
                    .ContinueWith(
                        t =>
                        {
                            if (t is { IsFaulted: true, Exception: { } ex })
                            {
                                self.RunOnPrimaryThread(() =>
                                {
                                    var flatten = ex.Flatten();
                                    self.UnhandledException?.Invoke(flatten);
                                });
                            }
                        },
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default
                    );
            },
            (action, this)
        );
    }

    public Task RunOnPrimaryThreadAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsOnGameThread)
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        Post(
            static stateObj =>
            {
                var (act, tcsLocal, ctx) = ((Action, TaskCompletionSource, GameThreadSynchronizationContext))stateObj!;

                try
                {
                    act();
                    tcsLocal.SetResult();
                }
                catch (Exception ex)
                {
                    ctx.UnhandledException?.Invoke(ex);
                    tcsLocal.SetException(ex);
                }
            },
            (action, tcs, this)
        );

        return tcs.Task;
    }

    public Task RunOnPrimaryThreadAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsOnGameThread)
        {
            return action();
        }

        var tcs = new TaskCompletionSource();

        Post(
            static stateObj =>
            {
                var (func, tcsLocal, ctx) = ((Func<Task>, TaskCompletionSource, GameThreadSynchronizationContext))
                    stateObj!;

                func()
                    .ContinueWith(t =>
                    {
                        if (t is { IsFaulted: true, Exception: { } ex })
                        {
                            ctx.UnhandledException?.Invoke(ex);
                            tcsLocal.SetException(ex);
                        }
                        else
                        {
                            tcsLocal.SetResult();
                        }
                    });
            },
            (action, tcs, this)
        );

        return tcs.Task;
    }

    public void Dispose()
    {
        _workItems.Clear();
    }
}
