/**
 * @file range.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.range;

import std;

namespace retro
{
    export template <typename Container, typename Reference>
    concept ContainerAppendable = requires(Container &c, Reference &&ref) {
        requires requires { c.emplace_back(std::forward<Reference>(ref)); } ||
                     requires { c.push_back(std::forward<Reference>(ref)); } ||
                     requires { c.emplace(c.end(), std::forward<Reference>(ref)); } ||
                     requires { c.insert(c.end(), std::forward<Reference>(ref)); };
    };

    export template <typename R, typename T>
    concept ContainerCompatibleRange =
        std::ranges::input_range<R> && std::convertible_to<std::ranges::range_reference_t<R>, T>;
} // namespace retro
