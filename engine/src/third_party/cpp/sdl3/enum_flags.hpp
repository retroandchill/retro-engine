/**
 * @file enum_flags.hpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#define SDL_DEFINE_ENUM_FLAGS(Exporter, EnumType)                                                                      \
    Exporter constexpr EnumType operator|(EnumType lhs, EnumType rhs) noexcept                                         \
    {                                                                                                                  \
        using U = std::underlying_type_t<EnumType>;                                                                    \
                                                                                                                       \
        return static_cast<EnumType>(static_cast<U>(lhs) | static_cast<U>(rhs));                                       \
    }
