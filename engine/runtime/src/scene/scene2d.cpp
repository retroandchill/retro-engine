//
// Created by fcors on 1/1/2026.
//

module retro.runtime;

namespace retro
{
    Entity & Scene2D::create_entity(const Transform &transform) noexcept
    {
        uint32 slot_index;

        if (!free_list_.empty())
        {
            slot_index = free_list_.back();
            free_list_.pop_back();
        }
        else
        {
            slot_index = static_cast<uint32>(slots_.size());
            slots_.emplace_back();
        }

        auto &[dense_index, generation, alive] = slots_[slot_index];

        dense_index = static_cast<uint32>(entities_.size());

        EntityID id{
            .index      = slot_index,
            .generation = generation
        };
        const auto &entity = entities_.emplace_back(std::make_unique<Entity>(id, transform));
        alive = true;

        return *entity;
    }

    void Scene2D::destroy_entity(EntityID id)
    {
        if (id.index >= slots_.size()) return;

        auto &[dense_index, generation, alive] = slots_[id.index];
        if (!alive || generation != id.generation) return;

        if (const uint32 last_index = slots_.size() - 1; dense_index != last_index)
        {
            slots_[dense_index] = std::move(slots_[last_index]);

            const auto &moved = *entities_[dense_index];
            auto [index, generation] = moved.id();
            auto &moved_slot = slots_[index];
            moved_slot.dense_index = dense_index;
        }

        entities_.pop_back();

        alive = false;
        generation++;
        free_list_.push_back(id.index);
    }
}
