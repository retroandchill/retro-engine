/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.runtime.world.scene;

namespace retro
{
    Scene::Scene(SceneDrawProxy &draw_proxy) : draw_proxy_{&draw_proxy}
    {
    }

    void Scene::destroy_node(SceneNode &node)
    {
        node.detach_from_parent();
        nodes_.remove(node);
    }

    void Scene::collect_draw_calls(const Vector2u viewport_size)
    {
        draw_proxy_->collect_all_draw_calls(*this, viewport_size);
    }

    std::span<SceneNode *const> Scene::nodes_of_type(const std::type_index type) const noexcept
    {
        return nodes_.nodes_of_type(type);
    }
} // namespace retro
