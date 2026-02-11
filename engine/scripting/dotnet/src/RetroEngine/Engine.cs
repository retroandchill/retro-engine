using System.Runtime.InteropServices;
using RetroEngine.Core.Async;
using RetroEngine.Logging;
using RetroEngine.Tickables;
using ZLinq;

namespace RetroEngine;

public sealed partial class Engine : IDisposable
{
    private readonly GameThreadSynchronizationContext _synchronizationContext;
    private readonly HashSet<ITickable> _tickables = [];
    public ulong FrameCount { get; private set; }

    private static Engine? _instance;
    public static Engine Instance =>
        _instance ?? throw new InvalidOperationException("Engine has not been initialized.");

    private Engine()
    {
        _synchronizationContext = new GameThreadSynchronizationContext();
        _synchronizationContext.UnhandledException += ex => Logger.Error(ex.ToString());
        SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
    }

    public static Engine Create()
    {
        if (_instance is not null)
        {
            throw new InvalidOperationException("Engine has already been initialized.");
        }

        Interlocked.Exchange(ref _instance, new Engine());
        return _instance;
    }

    public static void RequestShutdown(int exitCode = 0)
    {
        if (_instance is null)
            throw new InvalidOperationException("Engine has not been initialized.");
        _instance.Dispose();
        Interlocked.Exchange(ref _instance, null);
        NativeRequestShutdown(exitCode);
    }

    public void RegisterTickable(ITickable tickable)
    {
        _tickables.Add(tickable);
    }

    public void UnregisterTickable(ITickable tickable)
    {
        _tickables.Remove(tickable);
    }

    public int Tick(float deltaTime, int maxTasks)
    {
        foreach (var tickable in _tickables.AsValueEnumerable().Where(t => t.TickEnabled))
        {
            tickable.Tick(deltaTime);
        }
        var tasksCalled = _synchronizationContext.Pump(maxTasks);
        FrameCount++;
        return tasksCalled;
    }

    public void Dispose()
    {
        _synchronizationContext.Dispose();
        SynchronizationContext.SetSynchronizationContext(null);
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_engine_request_shutdown")]
    private static partial void NativeRequestShutdown(int exitCode);
}
