//
// Created by fcors on 12/29/2025.
//

export module retro.core:math.concepts;

import std;
import :strings;

namespace retro
{
    export template <typename T>
    concept Numeric =
        std::same_as<T, std::remove_cvref_t<T>> && std::is_arithmetic_v<T> && !std::same_as<T, bool> && !Char<T>;
}
