//
// Created by fcors on 1/2/2026.
//

export module retro.logging:record;

import :level;

namespace retro::logging
{
    export struct Record
    {
        Level level{};
        std::string logger_name{};

        std::string message{};

        std::chrono::system_clock::time_point timestamp{std::chrono::system_clock::now()};

        std::source_location location = std::source_location::current();
    };
} // namespace retro::logging