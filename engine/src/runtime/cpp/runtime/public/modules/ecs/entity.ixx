/**
 * @file entity.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.ecs.entity;

import std;
import retro.core.algorithm.hashing;

namespace retro
{
    export struct Entity
    {
        std::uint32_t index = 0;
        std::uint32_t generation = 0;

        friend bool operator==(const Entity &lhs, const Entity &rhs) noexcept = default;
    };

    export constexpr Entity null_entity = {.index = std::numeric_limits<std::uint32_t>::max(), .generation = 0};

    export struct EntitySlot
    {
        std::uint32_t generation = 0;
        bool alive = false;
    };

} // namespace retro

template <>
struct std::hash<retro::Entity>
{
    std::size_t operator()(const retro::Entity &entity) const noexcept
    {
        return retro::hash_combine(entity.index, entity.generation);
    }
};
