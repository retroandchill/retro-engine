//
// Created by fcors on 1/2/2026.
//

export module retro.logging:log_level;

import retro.core;
import std;
import spdlog;

namespace retro
{
    export enum class LogLevel : uint8
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Critical,
        Off
    };

    export constexpr std::string_view to_string(const LogLevel level)
    {
        switch (level)
        {
            using enum LogLevel;
            case Trace:
                return "trace";
            case Debug:
                return "debug";
            case Info:
                return "info";
            case Warn:
                return "warn";
            case Error:
                return "error";
            case Critical:
                return "critical";
            case Off:
                return "off";
        }
        return "unknown";
    }

    export constexpr LogLevel from_spd_level(spdlog::level::level_enum level)
    {
        switch (level)
        {
            using enum spdlog::level::level_enum;
            case trace:
                return LogLevel::Trace;
            case debug:
                return LogLevel::Debug;
            case info:
                return LogLevel::Info;
            case warn:
                return LogLevel::Warn;
            case err:
                return LogLevel::Error;
            case critical:
                return LogLevel::Critical;
            case off:
                return LogLevel::Off;
            default:
                return LogLevel::Off;
        }
    }

    export constexpr spdlog::level::level_enum to_spd_level(const LogLevel level)
    {
        switch (level)
        {
            using enum LogLevel;
            case Trace:
                return spdlog::level::trace;
            case Debug:
                return spdlog::level::debug;
            case Info:
                return spdlog::level::info;
            case Warn:
                return spdlog::level::warn;
            case Error:
                return spdlog::level::err;
            case Critical:
                return spdlog::level::critical;
            case Off:
                return spdlog::level::off;
            default:
                return spdlog::level::off;
        }
    }
} // namespace retro