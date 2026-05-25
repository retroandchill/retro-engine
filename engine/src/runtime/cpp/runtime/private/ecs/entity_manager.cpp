/**
 * @file entity_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.ecs.entity_manager;

namespace retro
{

    Entity EntityManager::create_entity()
    {
        if (!free_entities_.empty())
        {
            const std::uint32_t index = free_entities_.front();
            free_entities_.pop();

            auto &[generation, alive] = slots_[index];
            alive = true;
            return Entity{.index = index, .generation = generation};
        }

        const auto index = static_cast<std::uint32_t>(slots_.size());

        slots_.push_back(EntitySlot{.generation = 0, .alive = true});

        return Entity{.index = index, .generation = 0};
    }
    bool EntityManager::destroy_entity(Entity entity)
    {
        if (!is_alive(entity))
        {
            return false;
        }

        for (const auto &pool : component_pools_ | std::views::values)
        {
            pool->erase(entity);
        }

        auto &[generation, alive] = slots_[entity.index];
        alive = false;
        generation++;
        free_entities_.push(entity.index);

        return true;
    }

    bool EntityManager::is_alive(const Entity entity) const
    {
        if (entity.index >= slots_.size())
        {
            return false;
        }

        auto &[generation, alive] = slots_[entity.index];
        return alive && generation == entity.generation;
    }
} // namespace retro
