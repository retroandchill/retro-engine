/**
 * @file arena_allocator.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.memory.arena_allocator;

namespace retro
{
    constexpr std::size_t persistent_allocator_reserve_size = 2147483648;

    SingleArena &get_persistent_arena()
    {
        static SingleArena arena{persistent_allocator_reserve_size + 64 * 1024};
        return arena;
    }
} // namespace retro
