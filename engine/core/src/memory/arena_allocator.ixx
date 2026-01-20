/**
 * @file arena_allocator.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:memory.arena_allocator;

import :memory.arena;

namespace retro
{
    export template <typename T, Arena Allocator = SingleArena>
    class ArenaAllocator
    {
      public:
        using value_type = T;

        constexpr explicit ArenaAllocator(Allocator &arena) noexcept : arena_(std::addressof(arena))
        {
        }

        template <typename U>
        constexpr explicit ArenaAllocator(const ArenaAllocator<U, Allocator> &other) : arena_(other.arena_)
        {
        }

        T *allocate(const usize size)
        {
            auto *allocated = arena_->allocate(size * sizeof(T), alignof(T));
            if (allocated == nullptr)
                throw std::bad_alloc{};
            return static_cast<T *>(allocated);
        }

        std::allocation_result<T *> allocate_at_least(const usize size)
        {
            usize allocated_size = std::bit_ceil(size);
            auto *allocated = arena_->allocate(allocated_size * sizeof(T), alignof(T));
            if (allocated == nullptr)
            {
                allocated = arena_->allocate(size * sizeof(T), alignof(T));
                allocated_size = size;

                if (allocated == nullptr)
                    throw std::bad_alloc{};
            }

            return {static_cast<T *>(allocated), allocated_size};
        }

        void deallocate(T *, const usize) noexcept
        {
            // No manual deallocate, the entire arena is deallocated when it goes out of scope
        }

        usize max_size() const noexcept
        {
            return arena_->capacity();
        }

        friend bool operator==(const ArenaAllocator &, const ArenaAllocator &) noexcept = default;

      private:
        Allocator *arena_;

        template <typename U, Arena OtherAlloc>
        friend class ArenaAllocator;
    };

    export template <typename T, Arena Allocator>
    constexpr ArenaAllocator<T, Allocator> make_allocator(Allocator &allocator)
    {
        return ArenaAllocator<T, Allocator>(allocator);
    }

} // namespace retro
