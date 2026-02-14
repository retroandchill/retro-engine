/**
 * @file comparison.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.comparison;

import std;

namespace retro
{
    export template <typename T, typename U>
    concept EqualityComparableWith = requires(const T &t, const U &u) {
        {
            t == u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept InequalityComparableWith = requires(const T &t, const U &u) {
        {
            t != u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept LessThanComparableWith = requires(const T &t, const U &u) {
        {
            t < u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept GreaterThanComparableWith = requires(const T &t, const U &u) {
        {
            t > u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept LessThanOrEqualComparableWith = requires(const T &t, const U &u) {
        {
            t <= u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept GreaterThanOrEqualComparableWith = requires(const T &t, const U &u) {
        {
            t >= u
        } -> std::convertible_to<bool>;
    };
} // namespace retro
