/**
 * @file arena_allocator.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.core.memory.arena_allocator;

import std;
import retro.core.util.noncopyable;
import retro.core.memory.align;
import retro.core.memory.buffers;

namespace retro
{
    export template <typename T>
    concept Arena = requires(T arena, std::size_t size, std::size_t alignment) {
        {
            arena.allocate(size, alignment)
        } -> std::same_as<void *>;
        {
            arena.capacity()
        } -> std::same_as<std::size_t>;
        {
            arena.can_allocate(size, alignment)
        } -> std::convertible_to<bool>;
    };

    export template <typename T>
    concept ContiguousArena = Arena<T> && requires(T arena) {
        {
            arena.data()
        } -> std::same_as<void *>;
        {
            arena.used_capacity()
        } -> std::same_as<std::size_t>;
    };

    template <typename T>
    concept ResetableArena = Arena<T> && requires(T arena) { arena.reset(); };

    export class SingleArena
    {
      public:
        explicit constexpr SingleArena(const std::size_t size)
            : data_(std::make_unique<std::byte[]>(size)), capacity_{size}
        {
        }

        constexpr void *allocate(const std::size_t size,
                                 const std::size_t alignment = alignof(std::max_align_t)) // NOLINT
        {
            void *next = data_.get() + current_offset_;
            std::size_t remaining = capacity_ - current_offset_;
            auto *res = align(alignment, size, next, remaining);
            if (res != nullptr)
            {
                auto *new_pos = static_cast<std::byte *>(res) + size;
                current_offset_ = static_cast<std::size_t>(new_pos - data_.get());
                return res;
            }

            return nullptr;
        }

        [[nodiscard]] constexpr void *data() const noexcept
        {
            return data_.get();
        }

        constexpr void reset() noexcept
        {
            current_offset_ = 0;
        }

        [[nodiscard]] constexpr std::size_t capacity() const noexcept
        {
            return capacity_;
        }

        [[nodiscard]] constexpr std::size_t used_capacity() const noexcept
        {
            return current_offset_;
        }

        // NOLINTNEXTLINE
        [[nodiscard]] constexpr bool can_allocate(
            const std::size_t size,
            const std::size_t alignment = alignof(std::max_align_t)) const noexcept
        {
            if (size == 0)
            {
                return false;
            }

            void *next = data_.get() + current_offset_;
            std::size_t remaining = capacity_ - current_offset_;

            const void *res = align(alignment, size, next, remaining);
            return res != nullptr;
        }

      private:
        std::unique_ptr<std::byte[]> data_{};
        std::size_t capacity_{};
        std::size_t current_offset_{0};
    };

    export class MultiArena : NonCopyable
    {
      public:
        explicit constexpr MultiArena(const std::size_t block_capacity,
                                      const std::size_t initial_blocks = 10,
                                      const std::size_t max_blocks = std::numeric_limits<std::size_t>::max())
            : block_capacity_{block_capacity}, max_blocks_{max_blocks}
        {
            assert(block_capacity > alignof(std::max_align_t));
            assert(max_blocks_ > 0);
            assert(initial_blocks <= max_blocks_);
            blocks_.reserve(std::min(initial_blocks, max_blocks_));
            blocks_.emplace_back(block_capacity);
        }

        constexpr void *allocate(const std::size_t size,
                                 const std::size_t alignment = alignof(std::max_align_t)) // NOLINT
        {
            auto *block = &blocks_.back();

            if (!block->can_allocate(size, alignment))
            {
                if (blocks_.size() >= max_blocks_)
                {
                    throw std::bad_alloc{};
                }

                block = &blocks_.emplace_back(block_capacity_);
            }

            return block->allocate(size, alignment);
        }

        constexpr void reset() noexcept
        {
            while (blocks_.size() > 1)
                blocks_.pop_back();
            blocks_.back().reset();
        }

        [[nodiscard]] constexpr std::size_t capacity() const noexcept
        {
            return block_capacity_ * max_blocks_;
        }

        [[nodiscard]] constexpr bool can_allocate(
            const std::size_t size,
            const std::size_t alignment = alignof(std::max_align_t)) const noexcept
        {
            if (blocks_.size() < max_blocks_)
                return size <= block_capacity_;

            return blocks_.back().can_allocate(size, alignment);
        }

      private:
        std::vector<SingleArena> blocks_{};
        std::size_t block_capacity_{};
        std::size_t max_blocks_{};
    };

    export template <std::size_t N>
    class InlineArena : NonCopyable // NOLINT
    {
      public:
        constexpr void *allocate(const std::size_t size, const std::size_t alignment = alignof(std::max_align_t))
        {
            void *next = data_.data() + current_offset_;
            std::size_t remaining = N - current_offset_;
            if (auto *res = align(alignment, size, next, remaining); res != nullptr)
            {
                auto *new_pos = static_cast<std::byte *>(res) + size;
                current_offset_ = static_cast<std::size_t>(new_pos - data_.data());
                return res;
            }

            return nullptr;
        }

        constexpr void reset() noexcept
        {
            current_offset_ = 0;
        }

        [[nodiscard]] constexpr void *data() const noexcept
        {
            return data_.data();
        }

        [[nodiscard]] constexpr std::size_t capacity() const noexcept
        {
            return N;
        }

        [[nodiscard]] constexpr std::size_t used_capacity() const noexcept
        {
            return current_offset_;
        }

        [[nodiscard]] constexpr bool can_allocate(
            const std::size_t size,
            const std::size_t alignment = alignof(std::max_align_t)) const noexcept
        {
            if (size == 0)
            {
                return false;
            }

            void *next = data_.get() + current_offset_;
            std::size_t remaining = N - current_offset_;

            const void *res = align(alignment, size, next, remaining);
            return res != nullptr;
        }

      private:
        std::array<std::byte, N> data_;
        std::size_t current_offset_{0};
    };

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

        T *allocate(const std::size_t size)
        {
            auto *allocated = arena_->allocate(size * sizeof(T), alignof(T));
            if (allocated == nullptr)
                throw std::bad_alloc{};
            return static_cast<T *>(allocated);
        }

        std::allocation_result<T *> allocate_at_least(const std::size_t size)
        {
            std::size_t allocated_size = std::bit_ceil(size);
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

        void deallocate(T *, const std::size_t) noexcept
        {
            // No manual deallocate, the entire arena is deallocated when it goes out of scope
        }

        std::size_t max_size() const noexcept
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

    export template <Arena T>
    class ArenaMemoryResource final : public std::pmr::memory_resource
    {
      public:
        template <std::convertible_to<T> U>
        constexpr explicit ArenaMemoryResource(U &&arena) : arena_(std::forward<U>(arena))
        {
        }

        template <typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr explicit ArenaMemoryResource(std::in_place_t, Args &&...args) : arena_(std::forward<Args>(args)...)
        {
        }

        void reset() noexcept
            requires ResetableArena<T>
        {
            arena_.reset();
        }

      protected:
        void *do_allocate(size_t bytes, size_t align) override
        {
            return arena_.allocate(bytes, align);
        }

        void do_deallocate(void *ptr, size_t bytes, size_t align) override
        {
            // No-op for arena allocators
        }

        [[nodiscard]] bool do_is_equal(const memory_resource &other) const noexcept override
        {
            // Arena's a unique objects so they will never be equal
            return false;
        }

      private:
        T arena_;
    };

    export using SingleArenaMemoryResource = ArenaMemoryResource<SingleArena>;

    export using MultiArenaMemoryResource = ArenaMemoryResource<MultiArena>;

    export template <std::size_t Size>
    using InlineArenaMemoryResource = ArenaMemoryResource<InlineArena<Size>>;

} // namespace retro
