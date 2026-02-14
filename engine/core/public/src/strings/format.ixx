/**
 * @file format.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <fmt/format.h>
#include <fmt/xchar.h>

export module retro.core.strings.format;

import std;
import retro.core.type_traits.basic;

namespace retro
{
    export template <Char CharType, typename... Args>
    auto format(std::basic_format_string<CharType, Args...> fmt, Args &&...args)
    {
        return fmt::format(fmt, std::forward<Args>(args)...);
    }

    export template <Char CharType, typename... Args>
    auto vformat(std::basic_string_view<CharType> fmt, Args &&...args)
    {
        return fmt::vformat(fmt, std::forward<Args>(args)...);
    }

    export template <typename Output, Char CharType, typename... Args>
    auto format_to(Output &it, std::basic_format_string<CharType, Args...> fmt, Args &&...args)
    {
        return fmt::format_to(it, fmt, std::forward<Args>(args)...);
    }

    export template <typename Output, Char CharType, typename... Args>
    auto vformat_to(Output &it, std::basic_string_view<CharType> fmt, Args &&...args)
    {
        return fmt::vformat_to(it, fmt, std::forward<Args>(args)...);
    }
} // namespace retro
