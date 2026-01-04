// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Binds;

namespace RetroEngine.Logging;

[BlittableType("retro::LogLevel", CppModule = "retro.logging")]
public enum LogLevel : byte
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Critical,
    Off,
}
