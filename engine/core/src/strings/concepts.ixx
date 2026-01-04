/**
 * @file concepts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:strings.concepts;

import std;

namespace retro
{
    /**
     * Concept to ensure that the underlying type is a character.
     */
    export template <typename T>
    concept Char = std::same_as<T, char> || std::same_as<T, wchar_t> || std::same_as<T, char8_t> ||
                   std::same_as<T, char16_t> || std::same_as<T, char32_t>;
} // namespace retro
