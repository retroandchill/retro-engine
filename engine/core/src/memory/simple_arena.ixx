//
// Created by fcors on 1/6/2026.
//
module;

#include <cassert>

export module retro.core:memory.simple_arena;

import :defines;
import std;
import boost;

namespace retro
{
    struct ArenaBlock
    {
        std::unique_ptr<std::byte[]> data{};
        usize capacity{};
        usize current_offset{};

        explicit constexpr ArenaBlock(const usize size) : data(std::make_unique<std::byte[]>(size))
        {
        }
    };

    export class SimpleArena;

    export template <typename T>
        requires(!std::is_trivially_destructible_v<T>)
    class ArenaAllocated
    {
        explicit constexpr ArenaAllocated(T &ref) : ptr_{&ref}
        {
        }

      public:
        ArenaAllocated(const ArenaAllocated &) = delete;
        constexpr ArenaAllocated(ArenaAllocated &&other) noexcept : ptr_(other.ptr_)
        {
            other.ptr_ = nullptr;
        }

        ~ArenaAllocated() noexcept
        {
            if (ptr_ != nullptr)
            {
                std::destroy_at(ptr_);
            }
        }

        constexpr ArenaAllocated &operator=(const ArenaAllocated &) = delete;

        constexpr ArenaAllocated &operator=(ArenaAllocated &&other) noexcept
        {
            if (ptr_ != nullptr)
            {
                std::destroy_at(ptr_);
            }
            ptr_ = other.ptr_;
            other.ptr_ = nullptr;

            return *this;
        }

        constexpr T *operator->() const noexcept
        {
            return ptr_;
        }
        constexpr T &operator*() const noexcept
        {
            return &ptr_;
        }

        constexpr T *get() const noexcept
        {
            return ptr_;
        }

      private:
        friend class SimpleArena;

        T *ptr_{};
    };

    class SimpleArena : boost::noncopyable
    {
      public:
        explicit constexpr SimpleArena(const usize block_capacity,
                                       const usize initial_blocks = 10,
                                       const usize max_blocks = std::numeric_limits<usize>::max())
            : block_capacity_{block_capacity}, max_blocks_{max_blocks}
        {
            assert(block_capacity > alignof(std::max_align_t));
            assert(max_blocks_ > 0);
            assert(initial_blocks <= max_blocks_);
            blocks_.reserve(std::min(initial_blocks, max_blocks_));
            blocks_.emplace_back(block_capacity);
        }

        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr decltype(auto) allocate(Args &&...args)
        {
            return allocate_with_tail<T>(0, std::forward<Args>(args)...);
        }

        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        constexpr decltype(auto) allocate_with_tail(const usize tail_size, Args &&...args)
        {
            auto *block = &blocks_.back();

            constexpr usize alignment = alignof(T);
            usize aligned_offset = block->current_offset;
            if (const usize misalignment = aligned_offset % alignment; misalignment != 0)
            {
                aligned_offset += alignment - misalignment;
            }

            if (aligned_offset + sizeof(T) + tail_size > block->capacity)
            {
                if (blocks_.size() >= max_blocks_)
                {
                    throw std::bad_alloc{};
                }

                block = &blocks_.emplace_back(block_capacity_);
                aligned_offset = 0;
            }

            auto *ptr = std::bit_cast<T *>(block->data.get() + aligned_offset);
            std::construct_at(ptr, std::forward<Args>(args)...);
            block->current_offset = aligned_offset + sizeof(T) + tail_size;

            if constexpr (std::is_trivially_destructible_v<T>)
            {
                return *ptr;
            }
            else
            {
                return ArenaAllocated<T>{*ptr};
            }
        }

      private:
        std::vector<ArenaBlock> blocks_{};
        usize block_capacity_{};
        usize max_blocks_{};
    };
} // namespace retro
