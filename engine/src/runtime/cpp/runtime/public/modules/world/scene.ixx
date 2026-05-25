/**
 * @file scene.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.world.scene;

import std;
import retro.core.containers.optional;
import retro.core.functional.delegate;
import retro.core.util.noncopyable;
import retro.core.math.transform;
import retro.core.math.vector;
import retro.runtime.ecs.entity;
import retro.runtime.ecs.entity_manager;
import retro.runtime.world.scene_node;

namespace retro
{
    export class RETRO_API Scene final : NonCopyable
    {
      public:
        Entity create_entity();
        bool destroy_entity(Entity entity);

        void attach_entity(Entity child, Entity parent);
        bool detach_entity(Entity child);

        template <std::movable T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &add_component(Entity entity, Args &&...args)
        {
            return entities_.add<T>(entity, std::forward<Args>(args)...);
        }

        template <typename T>
        bool remove_component(const Entity entity)
        {
            return entities_.remove<T>(entity);
        }

        template <typename T>
        Optional<T &> try_get_component(const Entity entity)
        {
            return entities_.try_get<T>(entity);
        }

        template <typename T>
        Optional<const T &> try_get_component(const Entity entity) const
        {
            return entities_.try_get<T>(entity);
        }

        template <std::movable... Components>
        auto view()
        {
            return entities_.view<Components...>();
        }

        template <std::movable... Components>
        auto view() const
        {
            return entities_.view<Components...>();
        }

      private:
        EntityManager entities_;
        std::vector<Entity> root_entities_;
        std::unordered_map<Entity, std::size_t> root_entity_indices_;
    };

    export using OnSceneDelegate = MulticastDelegate<void(Scene &)>;

    export class RETRO_API SceneManager final : NonCopyable
    {
      public:
        Scene &create_scene();

        void destroy_scene(Scene &scene);

        [[nodiscard]] inline std::span<const std::unique_ptr<Scene>> scenes() const noexcept
        {
            return scenes_;
        }

        [[nodiscard]] inline OnSceneDelegate::Event on_scene_created() noexcept
        {
            return OnSceneDelegate::Event{on_scene_created_};
        }

        [[nodiscard]] inline OnSceneDelegate::Event on_scene_destroyed() noexcept
        {
            return OnSceneDelegate::Event{on_scene_destroyed_};
        }

      private:
        std::vector<std::unique_ptr<Scene>> scenes_;
        OnSceneDelegate on_scene_created_;
        OnSceneDelegate on_scene_destroyed_;
    };
} // namespace retro
