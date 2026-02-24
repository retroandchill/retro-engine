/**
 * @file margin.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.layout.margin;

namespace retro
{
    export struct Margin
    {
        float left;
        float top;
        float right;
        float bottom;

        constexpr friend bool operator==(const Margin &lhs, const Margin &rhs) noexcept = default;
    };
} // namespace retro
