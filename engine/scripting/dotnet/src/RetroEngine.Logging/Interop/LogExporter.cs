// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Binds;

namespace RetroEngine.Logging.Interop;

[BindExport("retro")]
internal static unsafe partial class LogExporter
{
    public static partial void Log(LogLevel level, [CppType(IsConst = true)] char* message, int length);
}
