/**
 * @file entity_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.ecs.entity_manager;

import std;
import retro.core.containers.optional;
import retro.runtime.ecs.entity;
import retro.runtime.ecs.component_pool;

namespace retro
{
    export class RETRO_API EntityManager
    {
      public:
        Entity create_entity();
        bool destroy_entity(Entity entity);

        [[nodiscard]] bool is_alive(Entity entity) const;

        template <std::movable T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &add(Entity entity, Args &&...args)
        {
            if (!is_alive(entity))
                throw std::invalid_argument{"Entity is not alive"};

            return get_pool<T>().emplace(entity, std::forward<Args>(args)...);
        }

        template <std::movable T>
        void remove(Entity entity)
        {
            if (auto existing_pool = try_get_pool<T>(); existing_pool.has_value())
            {
                existing_pool->erase(entity);
            }
        }

        template <std::movable T>
        Optional<T &> try_get(Entity entity) noexcept
        {
            if (!is_alive(entity))
                return std::nullopt;

            return try_get_pool<T>().and_then([entity](ComponentPoolImpl<T> &pool) { return pool.try_get(entity); });
        }

        template <std::movable T>
        Optional<const T &> try_get(Entity entity) const noexcept
        {
            if (!is_alive(entity))
                return std::nullopt;

            return try_get_pool<T>().and_then([entity](const ComponentPoolImpl<T> &pool)
                                              { return pool.try_get(entity); });
        }

        template <std::movable... Components>
        auto view();

      private:
        template <typename T>
        [[nodiscard]] ComponentPoolImpl<T> &get_pool()
        {
            const auto type = std::type_index{typeid(T)};

            auto [it, inserted] = component_pools_.try_emplace(type, nullptr);
            if (inserted)
            {
                it->second = std::make_unique<ComponentPoolImpl<T>>();
            }

            return static_cast<ComponentPoolImpl<T> &>(*it->second);
        }

        template <typename T>
        [[nodiscard]] Optional<ComponentPoolImpl<T> &> try_get_pool()
        {
            const auto it = component_pools_.find(typeid(T));
            if (it == component_pools_.end())
                return std::nullopt;

            return static_cast<ComponentPoolImpl<T> &>(*it->second);
        }

        template <typename T>
        [[nodiscard]] Optional<const ComponentPoolImpl<T> &> try_get_pool()
        {
            const auto it = component_pools_.find(typeid(T));
            if (it == component_pools_.end())
                return std::nullopt;

            return static_cast<ComponentPoolImpl<T> &>(*it->second);
        }

        std::vector<EntitySlot> slots_;
        std::queue<std::uint32_t> free_entities_;
        std::unordered_map<std::type_index, std::unique_ptr<ComponentPool>> component_pools_;
    };
} // namespace retro
