// @file GameModule.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace RetroEngine.Game.Sample;

internal static class GameModule
{
    [ModuleInitializer]
    public static void InitEngineLoading()
    {
#if RETRO_WITH_EDITOR
        var engineBasePath = Environment.GetEnvironmentVariable("RETRO_ENGINE_BINARY_PATH");
        AssemblyLoadContext.Default.Resolving += (_, assemblyName) =>
        {
            var searchPath = Path.Join(engineBasePath, $"{assemblyName.Name}.dll");
            return File.Exists(searchPath) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(searchPath) : null;
        };
#endif
    }
}
