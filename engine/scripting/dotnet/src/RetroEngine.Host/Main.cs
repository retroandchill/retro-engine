// @file $Main.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Runtime.InteropServices;
using RetroEngine.Core;
using RetroEngine.Logging;

namespace RetroEngine.Host;

public static class Main
{
    [UnmanagedCallersOnly]
    public static unsafe int InitializeScriptEngine(char* workingDirectoryPath, int workingDirectoryPathLength)
    {
        try
        {
            AppDomain.CurrentDomain.SetData(
                "APP_CONTEXT_BASE_DIRECTORY",
                new ReadOnlySpan<char>(workingDirectoryPath, workingDirectoryPathLength).ToString()
            );

            Logger.Info("Script engine initialized successfully.");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}
