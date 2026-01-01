//
// Created by fcors on 12/31/2025.
//

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