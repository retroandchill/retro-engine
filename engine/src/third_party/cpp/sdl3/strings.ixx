/**
 * @file strings.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module sdl:strings;

import std;

namespace sdl
{
    template <typename T>
    concept StringParam = std::convertible_to<T, const char *> || requires(T &&t) {
        {
            std::forward<T>(t).c_str()
        } -> std::convertible_to<const char *>;
    };

    template <StringParam T>
    constexpr const char *to_cstring(T &&str)
    {
        if constexpr (std::convertible_to<T, const char *>)
        {
            return std::forward<T>(str);
        }
        else
        {
            return std::forward<T>(str).c_str();
        }
    }
} // namespace sdl
