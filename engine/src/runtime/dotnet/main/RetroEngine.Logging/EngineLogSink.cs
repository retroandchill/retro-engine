// // @file EngineLogSink.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Interop;
using Serilog.Core;
using Serilog.Events;

namespace RetroEngine.Logging;

public sealed partial class EngineLogSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var level = logEvent.Level switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Info,
            LogEventLevel.Warning => LogLevel.Warn,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.Off,
        };

        var message = logEvent.RenderMessage();

        if (
            logEvent.Properties.GetValueOrDefault("Method") is ScalarValue { Value: string name }
            && logEvent.Properties.GetValueOrDefault("SourceFile") is ScalarValue { Value: string file }
            && logEvent.Properties.GetValueOrDefault("LineNumber") is ScalarValue { Value: int line }
        )
        {
            NativeLog(level, message, message.Length, name, name.Length, file, file.Length, line);
        }
        else
        {
            NativeLog(level, message, message.Length);
        }
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_log")]
    private static unsafe partial void NativeLog(LogLevel level, ReadOnlySpan<char> message, int length);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_log_with_source_info")]
    private static unsafe partial void NativeLog(
        LogLevel level,
        ReadOnlySpan<char> message,
        int length,
        ReadOnlySpan<char> methodName,
        int methodNameLength,
        ReadOnlySpan<char> sourceFile,
        int sourceFileLength,
        int sourceLine
    );
}
