// // @file LibraryLoader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RetroEngine.Interop;

public static class LibraryLoader
{
    private static readonly string OptionalSuffix;
    private static readonly string? OptionalPrefix;
    private static readonly ConcurrentDictionary<string, IntPtr> LoadedLibraries = new(
        StringComparer.OrdinalIgnoreCase
    );

    static LibraryLoader()
    {
        if (OperatingSystem.IsWindows())
        {
            OptionalSuffix = ".dll";
            OptionalPrefix = null;
        }
        else if (OperatingSystem.IsMacOS())
        {
            OptionalSuffix = ".dylib";
            OptionalPrefix = "lib";
        }
        else
        {
            OptionalSuffix = ".so";
            OptionalPrefix = "lib";
        }
    }

    public static void RegisterRetroInteropLoader(Assembly assembly)
    {
        NativeLibrary.SetDllImportResolver(assembly, RetroInteropResolver);
    }

    private static IntPtr RetroInteropResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return LoadedLibraries.GetOrAdd(
            libraryName,
            static libName =>
            {
                var primarySearchPath = Path.Join(
                    Path.GetDirectoryName(typeof(LibraryLoader).Assembly.Location),
                    "runtimes",
                    RuntimeInformation.RuntimeIdentifier,
                    "native"
                );

                ReadOnlySpan<string> candidates = OptionalPrefix is not null
                    ?
                    [
                        Path.Join(primarySearchPath, libName),
                        Path.Join(primarySearchPath, $"{libName}{OptionalSuffix}"),
                        Path.Join(primarySearchPath, $"{OptionalPrefix}{libName}"),
                        Path.Join(primarySearchPath, $"{OptionalPrefix}{libName}{OptionalSuffix}"),
                    ]
                    :
                    [
                        Path.Join(primarySearchPath, libName),
                        Path.Join(primarySearchPath, $"{libName}{OptionalSuffix}"),
                    ];

                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        return NativeLibrary.Load(candidate);
                    }
                }

                return IntPtr.Zero;
            }
        );
    }
}
