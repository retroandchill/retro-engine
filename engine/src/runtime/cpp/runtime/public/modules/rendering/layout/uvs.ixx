/**
 * @file uvs.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.layout.uvs;

import retro.core.math.vector;

namespace retro
{
    export struct UVs
    {
        Vector2f min{0, 0};
        Vector2f max{1, 1};
    };
} // namespace retro
