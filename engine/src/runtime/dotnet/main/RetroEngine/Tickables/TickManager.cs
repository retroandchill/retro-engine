// @file TickManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using RetroEngine.Async;
using RetroEngine.Events;
using RetroEngine.Utilities.Async;
using Serilog;
using ZLinq;

namespace RetroEngine.Tickables;

[RegisterSingleton]
public sealed class TickManager : IDisposable
{
    internal static TickManager? Instance { get; set; }

    private readonly EventManager _eventManager;

    private readonly Dictionary<TickGroup, HashSet<ITickable>> _tickables = new()
    {
        [TickGroup.Input] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
        [TickGroup.PreSimulation] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
        [TickGroup.Simulation] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
        [TickGroup.PostSimulation] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
        [TickGroup.UiLayout] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
        [TickGroup.PreRender] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
        [TickGroup.Render] = new HashSet<ITickable>(ReferenceEqualityComparer.Default),
    };
    private readonly GameThreadSynchronizationContext _synchronizationContext = new();
    private readonly NativeTaskScheduler _nativeTaskScheduler = new();

    public IThreadSync ThreadSync => _synchronizationContext;
    public ulong FrameCount { get; private set; }

    public TickManager(EventManager eventManager)
    {
        _eventManager = eventManager;
        _synchronizationContext.UnhandledException += OnUnhandledException;
    }

    public void RegisterTickable(ITickable tickable)
    {
        _tickables[tickable.TickGroup].Add(tickable);
    }

    public void UnregisterTickable(ITickable tickable)
    {
        _tickables[tickable.TickGroup].Remove(tickable);
    }

    internal SyncContextScope BindSynchronizationContext()
    {
        _synchronizationContext.AssignToGameThread();
        return new SyncContextScope(_synchronizationContext, _nativeTaskScheduler);
    }

    private static void OnUnhandledException(Exception ex)
    {
        Log.Error(ex, "Unhandled exception in game thread.");
    }

    internal void Tick(float deltaTime)
    {
        _eventManager.PollEvents();
        Tick(TickGroup.Input, deltaTime);
        Tick(TickGroup.PreSimulation, deltaTime);
        _nativeTaskScheduler.PumpTasks();
        Tick(TickGroup.Simulation, deltaTime);
        _synchronizationContext.Pump();
        Tick(TickGroup.PostSimulation, deltaTime);
        FrameCount++;
        Tick(TickGroup.UiLayout, deltaTime);
        Tick(TickGroup.PreRender, deltaTime);
        Tick(TickGroup.Render, deltaTime);
    }

    private void Tick(TickGroup group, float deltaTime)
    {
        foreach (var tickable in _tickables[group].AsValueEnumerable().Where(t => t.TickEnabled))
        {
            tickable.Tick(deltaTime);
        }
    }

    internal ref struct SyncContextScope : IDisposable
    {
        private bool _disposed;
        private readonly SynchronizationContext? _previous;
        private NativeTaskScheduler.Scope _nativeScope;

        public SyncContextScope(SynchronizationContext context, NativeTaskScheduler scheduler)
        {
            _previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            _nativeScope = scheduler.CreateThreadScope();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            SynchronizationContext.SetSynchronizationContext(_previous);
            _nativeScope.Dispose();
            _disposed = true;
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
