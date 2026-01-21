/**
 * @file casts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:casts;

import std;

namespace retro
{
    export template <typename From, typename To>
    concept CanStaticCast = requires(From &&from) {
        {
            static_cast<To>(std::forward<From>(from))
        } -> std::same_as<To>;
    };
}
