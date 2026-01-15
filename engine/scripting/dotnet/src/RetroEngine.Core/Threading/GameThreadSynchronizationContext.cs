// // @file GameThreadSynchronizationContext.cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Core.Threading;

public sealed class GameThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly int _gameThreadId = Environment.CurrentManagedThreadId;
    private readonly Queue<IWorkItem> _workItems = new();
    private readonly Lock _lock = new();

    public bool IsOnGameThread => _gameThreadId == Environment.CurrentManagedThreadId;

    public event Action<Exception>? UnhandledException;

    public override void Post(SendOrPostCallback d, object? state)
    {
        ArgumentNullException.ThrowIfNull(d);
        using var scope = _lock.EnterScope();
        _workItems.Enqueue(new PostWorkItem(d, state));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (IsOnGameThread)
        {
            d(state);
            return;
        }

        using var done = new ManualResetEventSlim(false);
        Exception? exception = null;

        using (_lock.EnterScope())
        {
            _workItems.Enqueue(new SendWorkItem(d, state, done, ex => exception = ex));
        }

        done.Wait();
        if (exception is not null)
        {
            throw new AggregateException("Exception during SynchronizationContext.Send.", exception);
        }
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
            IWorkItem? workItem;

            using (_lock.EnterScope())
            {
                if (_workItems.Count == 0)
                    break;
                workItem = _workItems.Dequeue();
            }

            try
            {
                workItem.Execute();
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
        using var scope = _lock.EnterScope();
        _workItems.Clear();
    }

    private interface IWorkItem
    {
        void Execute();
    }

    private sealed class PostWorkItem(SendOrPostCallback d, object? state) : IWorkItem
    {
        public void Execute() => d(state);
    }

    private sealed class SendWorkItem(
        SendOrPostCallback d,
        object? state,
        ManualResetEventSlim done,
        Action<Exception> captureException
    ) : IWorkItem
    {
        public void Execute()
        {
            try
            {
                d(state);
            }
            catch (Exception ex)
            {
                captureException(ex);
            }
            finally
            {
                done.Set();
            }
        }
    }
}
