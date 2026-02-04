/**
 * @file rect.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.math.rect;

import retro.core.type_traits.basic;
import std;

namespace retro
{
    export template <Numeric T>
    struct Rect
    {
        T x;
        T y;
        T width;
        T height;
    };

    export using RectI = Rect<std::int32_t>;
    export using RectU = Rect<std::uint32_t>;
    export using RectF = Rect<float>;
} // namespace retro
