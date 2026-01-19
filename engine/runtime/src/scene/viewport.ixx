/**
 * @file viewport.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.viewport;

import std;
import :scene.rendering;
import :scene.transform;
import entt;

namespace retro
{
    export struct Viewport
    {
        Vector2f view_size{};
        Vector2f offset{};
        entt::entity root_node = entt::null;
    };
} // namespace retro
