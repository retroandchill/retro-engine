/**
 * @file macros.hpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#define RETRO_CONCAT_IMPL(a, b) a##b
#define RETRO_CONCAT(a, b) RETRO_CONCAT_IMPL(a, b)

#define EXPECT(expr)                                                                                                   \
    auto &&RETRO_CONCAT(__try_res, __LINE__) = (expr);                                                                 \
    if (!RETRO_CONCAT(__try_res, __LINE__).has_value())                                                                \
        return std::unexpected(                                                                                        \
            std::forward<decltype(RETRO_CONCAT(__try_res, __LINE__))>(RETRO_CONCAT(__try_res, __LINE__)).error());

#define EXPECT_ASSIGN(lhs, expr)                                                                                       \
    EXPECT(expr)                                                                                                       \
    lhs = *std::forward<decltype(RETRO_CONCAT(__try_res, __LINE__))>(RETRO_CONCAT(__try_res, __LINE__));

#define DECLARE_OPAQUE_C_HANDLE(handle, type)                                                                          \
    template <>                                                                                                        \
    struct retro::CHandleTraits<handle>                                                                                \
    {                                                                                                                  \
        using CppType = type;                                                                                          \
        static constexpr retro::CHandleType HandleType = retro::CHandleType::opaque;                                   \
    };                                                                                                                 \
    template <>                                                                                                        \
    struct retro::CAliasableTraits<type>                                                                               \
    {                                                                                                                  \
        using CType = handle;                                                                                          \
    };

#define DECLARE_DEFINED_C_HANDLE(handle, type)                                                                         \
    template <>                                                                                                        \
    struct retro::CHandleTraits<handle>                                                                                \
    {                                                                                                                  \
        using CppType = type;                                                                                          \
        static constexpr retro::CHandleType HandleType = retro::CHandleType::defined;                                  \
    };                                                                                                                 \
    template <>                                                                                                        \
    struct retro::CAliasableTraits<type>                                                                               \
    {                                                                                                                  \
        using CType = handle;                                                                                          \
    };
