//
// Created by fcors on 1/2/2026.
//

export module retro.logging:level;

import retro.core;
import std;

namespace retro::logging
{
    export enum class Level : uint8
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Critical,
        Off
    };

    export constexpr std::string_view to_string(const Level level)
    {
        switch (level)
        {
            using enum Level;
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
} // namespace retro::logging