// @file Main.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using RetroEngine.Core.Async;
using RetroEngine.Host.Interop;
using RetroEngine.Logging;
using RetroEngine.SceneView;

namespace RetroEngine.Host;

public static class Main
{
    private static GameThreadSynchronizationContext? _synchronizationContext;
    private static IGameSession? _gameSession;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe int InitializeScriptEngine(
        char* workingDirectoryPath,
        int workingDirectoryPathLength,
        ScriptingCallbacks* callbacks
    )
    {
        try
        {
            AppDomain.CurrentDomain.SetData(
                "APP_CONTEXT_BASE_DIRECTORY",
                new ReadOnlySpan<char>(workingDirectoryPath, workingDirectoryPathLength).ToString()
            );

            *callbacks = new ScriptingCallbacks()
            {
                Start = &StartGame,
                Tick = &Tick,
                Exit = &ShutdownScriptEngine,
            };

            _synchronizationContext = new GameThreadSynchronizationContext();
            _synchronizationContext.UnhandledException += ex => Logger.Error(ex.ToString());

            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            var currentAssembly = Assembly.GetExecutingAssembly();
            var loadContext = AssemblyLoadContext.GetLoadContext(currentAssembly);
            if (loadContext is null)
            {
                Logger.Error("Could not find assembly load context for entry assembly.");
                return 1;
            }

            loadContext.Resolving += (context, name) =>
            {
                var candidatePath = Path.Combine(AppContext.BaseDirectory, $"{name.Name}.dll");
                return File.Exists(candidatePath) ? context.LoadFromAssemblyPath(candidatePath) : null;
            };

            Logger.Info("Script engine initialized successfully.");
            return 0;
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
            return 1;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe int StartGame(char* assemblyPath, int assemblyPathLength, char* className, int classNameLength)
    {
        try
        {
            if (_gameSession is not null)
            {
                Logger.Error("Game session is already running.");
                return 1;
            }

            var assemblyPathString = new ReadOnlySpan<char>(assemblyPath, assemblyPathLength);
            var classNameString = new ReadOnlySpan<char>(className, classNameLength);

            var currentAssembly = Assembly.GetExecutingAssembly();
            var loadContext = AssemblyLoadContext.GetLoadContext(currentAssembly);
            if (loadContext is null)
            {
                Logger.Error("Could not find assembly load context for entry assembly.");
                return 1;
            }

            var baseDirectory = AppContext.BaseDirectory;
            var absoluteAssemblyPath = Path.GetFullPath(assemblyPathString.ToString(), baseDirectory);

            var assembly = loadContext.LoadFromAssemblyPath(absoluteAssemblyPath);
            var type = assembly.GetType(classNameString.ToString());
            if (type is null)
            {
                Logger.Error($"Could not find type '{classNameString}' in assembly '{assemblyPathString}'.");
                return 1;
            }

            _gameSession = Activator.CreateInstance(type) as IGameSession;
            if (_gameSession is null)
            {
                Logger.Error(
                    $"Could not create instance of type '{classNameString}' in assembly '{assemblyPathString}'."
                );
                return 1;
            }

            _gameSession.Start();
            return 0;
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
            return 1;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static int Tick(float deltaTime, int maxTasks)
    {
        var tasksCalled = _synchronizationContext?.Pump(maxTasks) ?? 0;
        Scene.Sync();
        return tasksCalled;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void ShutdownScriptEngine()
    {
        _gameSession?.Terminate();
        _gameSession = null;

        _synchronizationContext?.Dispose();
        _synchronizationContext = null;
    }
}
