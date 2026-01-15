// @file $Main.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RetroEngine.Core.Threading;
using RetroEngine.Host.Interop;
using RetroEngine.Logging;

namespace RetroEngine.Host;

public static class Main
{
    private static GameThreadSynchronizationContext? _synchronizationContext;
    private static GameContext? _gameContext;

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
    public static unsafe int StartGame(
        char* assemblyPath,
        int assemblyPathLength,
        char* className,
        int classNameLength,
        char* methodName,
        int methodNameLength
    )
    {
        try
        {
            var assemblyPathString = new ReadOnlySpan<char>(assemblyPath, assemblyPathLength);
            var classNameString = new ReadOnlySpan<char>(className, classNameLength);
            var methodNameString = new ReadOnlySpan<char>(methodName, methodNameLength);

            var assembly = Assembly.LoadFrom(assemblyPathString.ToString());
            var type = assembly.GetType(classNameString.ToString());
            if (type is null)
            {
                Logger.Error($"Could not find type '{classNameString}' in assembly '{assemblyPathString}'.");
                return 1;
            }

            var method = type.GetMethod(methodNameString.ToString());
            if (method is null)
            {
                Logger.Error($"Could not find method '{methodNameString}' in type '{classNameString}'.");
                return 1;
            }

            var taskDelegate = method.CreateDelegate<Func<CancellationToken, Task<int>>>();

            _gameContext = new GameContext(taskDelegate);

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
        return _synchronizationContext?.Pump(maxTasks) ?? 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void ShutdownScriptEngine()
    {
        _gameContext?.Dispose();
        _synchronizationContext?.Dispose();
        _synchronizationContext = null;
    }
}
