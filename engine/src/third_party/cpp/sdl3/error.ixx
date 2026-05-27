/**
 * @file error.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL_error.h>

export module sdl:error;

import std;
import :strings;
import :utils;

namespace sdl
{
    export template <StringParam T>
    bool set_error(T &&message)
    {
        return SDL_SetError("%s", sdl::to_cstring(std::forward<T>(message)));
    }

    export template <typename... Args>
        requires(sizeof...(Args) > 0)
    bool set_error(std::format_string<Args...> fmt, Args &&...args)
    {
        return set_error(std::format(fmt, std::forward<Args>(args)...));
    }

    export template <typename... Args>
        requires(sizeof...(Args) > 0)
    bool set_error(const std::string_view fmt, Args &&...args)
    {
        return set_error(std::vformat(fmt, std::make_format_args(std::forward<Args>(args)...)));
    }

    export inline bool out_of_memory()
    {
        return SDL_OutOfMemory();
    }

    export inline const char *get_error() noexcept
    {
        return SDL_GetError();
    }

    export class Error : public std::exception
    {
      public:
        Error() = default;

        explicit inline Error(std::string message) : message_{std::move(message)}
        {
        }

        [[nodiscard]] constexpr const char *what() const noexcept override
        {
            return message_.c_str();
        }

        [[nodiscard]] constexpr const std::string &message() const noexcept
        {
            return message_;
        }

      private:
        std::string message_{get_error()};
    };

    export constexpr void check_error(const bool result)
    {
        if (!result)
            throw Error{};
    }

    export template <Negatable T>
    constexpr T check_error(T result)
    {
        if (!result)
            throw Error{};
        return result;
    }

    export template <std::equality_comparable T>
    constexpr T check_error(T result, T invalid_value)
    {
        if (result == invalid_value)
            throw Error{};

        return result;
    }

    export template <std::equality_comparable T>
    constexpr void check_error_if_not(T result, T expected)
    {
        if (result != expected)
            throw Error{};
    }

    export inline bool clear_error()
    {
        return SDL_ClearError();
    }
} // namespace sdl
