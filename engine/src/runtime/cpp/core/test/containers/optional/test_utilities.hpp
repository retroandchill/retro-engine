/**
 * @file test_utilities.hpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

namespace retro::tests
{
    /***
     * Evaluate and return an expression in a consteval context for testing
     * constexpr correctness.
     */
    auto consteval constify(auto expr)
    {
        return (expr);
    }
} // namespace retro::tests
