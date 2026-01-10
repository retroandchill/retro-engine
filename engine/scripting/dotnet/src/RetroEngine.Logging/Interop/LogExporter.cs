// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Logging.Interop;

internal static unsafe partial class LogExporter
{
    private const string LibraryName = "retro_logging";

    [LibraryImport(LibraryName, EntryPoint = "retro_log")]
    public static partial void Log(LogLevel level, char* message, int length);
}
