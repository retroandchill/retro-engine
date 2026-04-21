// // @file TickManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using RetroEngine.Async;
using RetroEngine.Utilities.Async;
using ZLinq;

namespace RetroEngine.Tickables;

[RegisterSingleton]
public sealed class TickManager : IDisposable
{
    private readonly HashSet<ITickable> _tickables = new(ReferenceEqualityComparer.Default);
    private readonly GameThreadSynchronizationContext _synchronizationContext = new();
    private readonly ILogger<TickManager> _logger;

    public IThreadSync ThreadSync => _synchronizationContext;
    public ulong FrameCount { get; private set; }

    public TickManager(ILogger<TickManager> logger)
    {
        _logger = logger;
        _synchronizationContext.UnhandledException += OnUnhandledException;
    }

    public void RegisterTickable(ITickable tickable)
    {
        _tickables.Add(tickable);
    }

    public void UnregisterTickable(ITickable tickable)
    {
        _tickables.Remove(tickable);
    }

    public void BindSynchronizationContext()
    {
        SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
    }

    private void OnUnhandledException(Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception in game thread.");
    }

    public void Tick(float deltaTime)
    {
        foreach (var tickable in _tickables.AsValueEnumerable().Where(t => t.TickEnabled))
        {
            tickable.Tick(deltaTime);
        }
        _synchronizationContext.Pump();
        FrameCount++;
    }

    public Task WaitFrame(ulong frameCount = 1, CancellationToken cancellationToken = default)
    {
        if (frameCount == 0)
        {
            return Task.CompletedTask;
        }

        var awaiter = new TickAwait(this, frameCount, cancellationToken);
        return awaiter.Task;
    }

    public async Task Timeline(float duration, Action<float> onTick, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(0, duration);

        if (duration == 0)
        {
            return;
        }

        var timeline = new Timeline(duration, onTick, cancellationToken);
        RegisterTickable(timeline);
        try
        {
            await timeline.Task;
        }
        finally
        {
            UnregisterTickable(timeline);
        }
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<ITickable>
    {
        public static readonly ReferenceEqualityComparer Default = new();

        public bool Equals(ITickable? x, ITickable? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(ITickable obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public void Dispose()
    {
        _synchronizationContext.Dispose();
    }
}
