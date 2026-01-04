/**
 * @file concepts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:math.concepts;

import std;
import :strings;

namespace retro
{
    export template <typename T>
    concept Numeric =
        std::same_as<T, std::remove_cvref_t<T>> && std::is_arithmetic_v<T> && !std::same_as<T, bool> && !Char<T>;
}
