// // @file EngineLoggingExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Serilog;
using Serilog.Enrichers.CallerInfo;

namespace RetroEngine.Logging;

public static class EngineLoggingExtensions
{
    public static LoggerConfiguration WithEngineLog(this LoggerConfiguration config)
    {
        return config.Enrich.WithCallerInfo(includeFileInfo: true, "RetroEngine.").WriteTo.Sink(new EngineLogSink());
    }
}
