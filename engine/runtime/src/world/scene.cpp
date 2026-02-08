/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.world.scene;

namespace retro
{
    void Scene::destroy_node(SceneNode &node)
    {
        node.detach_from_parent();
        nodes_.remove(node);
    }

    std::span<SceneNode *const> Scene::nodes_of_type(const std::type_index type) const noexcept
    {
        return nodes_.nodes_of_type(type);
    }

    Scene &SceneManager::create_scene()
    {
        const auto &created_scene = scenes_.emplace_back(std::make_unique<Scene>());
        on_scene_created_(*created_scene);
        return *created_scene;
    }

    void SceneManager::destroy_scene(Scene &scene)
    {
        const auto it = std::ranges::find_if(scenes_,
                                             [&scene](const std::unique_ptr<Scene> &ptr)
                                             { return ptr.get() == std::addressof(scene); });

        if (it != scenes_.end())
        {
            on_scene_destroyed_(**it);
            scenes_.erase(it);
        }
    }
} // namespace retro
