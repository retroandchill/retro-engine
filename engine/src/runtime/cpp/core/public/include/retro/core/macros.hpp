/**
 * @file macros.hpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#define RETRO_CONCAT_IMPL(a, b) a##b
#define RETRO_CONCAT(a, b) RETRO_CONCAT_IMPL(a, b)

#define EXPECT_IMPL(expr, return_statement, await_statement)                                                           \
    auto &&RETRO_CONCAT(__try_res, __LINE__) = (await_statement(expr));                                                \
    if (!RETRO_CONCAT(__try_res, __LINE__).has_value())                                                                \
        return_statement std::unexpected(                                                                              \
            std::forward<decltype(RETRO_CONCAT(__try_res, __LINE__))>(RETRO_CONCAT(__try_res, __LINE__)).error());

#define EXPECT(expr) EXPECT_IMPL(expr, return, )

#define CO_EXPECT(expr) EXPECT_IMPL(expr, co_return, )

#define AWAIT_EXPECT(expr) EXPECT_IMPL(expr, co_return, co_await)

#define EXPECT_ASSIGN(lhs, expr)                                                                                       \
    EXPECT(expr)                                                                                                       \
    lhs = *std::forward<decltype(RETRO_CONCAT(__try_res, __LINE__))>(RETRO_CONCAT(__try_res, __LINE__));

#define CO_EXPECT_ASSIGN(lhs, expr)                                                                                    \
    CO_EXPECT(expr)                                                                                                    \
    lhs = *std::forward<decltype(RETRO_CONCAT(__try_res, __LINE__))>(RETRO_CONCAT(__try_res, __LINE__));

#define AWAIT_EXPECT_ASSIGN(lhs, expr)                                                                                 \
    AWAIT_EXPECT(expr)                                                                                                 \
    lhs = *std::forward<decltype(RETRO_CONCAT(__try_res, __LINE__))>(RETRO_CONCAT(__try_res, __LINE__));
