// // @file 2026.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Logging.Interop;

namespace RetroEngine.Logging;

public static class Logger
{
    public static void Log(LogLevel level, ReadOnlySpan<char> message)
    {
        unsafe
        {
            fixed (char* messagePtr = message)
            {
                LogExporter.Log(level, messagePtr, message.Length);
            }
        }
    }

    public static void Trace(ReadOnlySpan<char> message) => Log(LogLevel.Trace, message);

    public static void Debug(ReadOnlySpan<char> message) => Log(LogLevel.Debug, message);

    public static void Info(ReadOnlySpan<char> message) => Log(LogLevel.Info, message);

    public static void Warn(ReadOnlySpan<char> message) => Log(LogLevel.Warn, message);

    public static void Error(ReadOnlySpan<char> message) => Log(LogLevel.Error, message);

    public static void Critical(ReadOnlySpan<char> message) => Log(LogLevel.Critical, message);
}
