// @file $Main.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Runtime.InteropServices;
using RetroEngine.Binds;
using RetroEngine.Core;

namespace RetroEngine.Host;

public static class Main
{
    [UnmanagedCallersOnly]
    public static unsafe NativeBool InitializeScriptEngine(
        char* workingDirectoryPath,
        int workingDirectoryPathLength,
        IntPtr bindsCallbacks
    )
    {
        try
        {
            AppDomain.CurrentDomain.SetData(
                "APP_CONTEXT_BASE_DIRECTORY",
                new ReadOnlySpan<char>(workingDirectoryPath, workingDirectoryPathLength).ToString()
            );

            Console.WriteLine("Script engine initialized successfully.");
            BindsManager.Initialize(bindsCallbacks);
            return NativeBool.True;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return NativeBool.False;
        }
    }
}
