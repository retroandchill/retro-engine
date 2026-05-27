/**
 * @file utils.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module sdl:utils;

import std;

namespace sdl
{

    template <typename T>
    concept Negatable = requires(T value) {
        {
            !value
        } -> std::convertible_to<bool>;
    };
}
