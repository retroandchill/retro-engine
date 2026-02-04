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
    Scene::Scene(PipelineManager &pipeline_manager) : pipeline_manager_{&pipeline_manager}
    {
    }

    void Scene::destroy_node(SceneNode &node)
    {
        node.detach_from_parent();
        nodes_.remove(node);
    }

    void Scene::collect_draw_calls(const Vector2u viewport_size) const
    {
        pipeline_manager_->collect_all_draw_calls(nodes_, viewport_size);
    }

    std::span<SceneNode *const> Scene::nodes_of_type(const std::type_index type) const noexcept
    {
        return nodes_.nodes_of_type(type);
    }
} // namespace retro
