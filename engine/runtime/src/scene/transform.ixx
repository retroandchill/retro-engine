/**
 * @file transform.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.transform;

import retro.core;

namespace retro
{
    export struct Transform
    {
        Vector2f position{};
        float rotation{0};
        Vector2f scale{1, 1};
    };
} // namespace retro
