/**
 * @file anchors.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.layout.anchors;

import retro.core.math.vector;

namespace retro
{
    export struct Anchors
    {
        Vector2f minimum;
        Vector2f maximum;

        constexpr friend bool operator==(const Anchors &lhs, const Anchors &rhs) noexcept = default;
    };
} // namespace retro
