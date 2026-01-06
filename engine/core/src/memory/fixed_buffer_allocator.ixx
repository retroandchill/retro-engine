/**
 * @file fixed_buffer_allocator.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:memory.fixed_buffer_allocator;

import :defines;

namespace retro
{
    export template <typename T>
    struct FixedBufferAllocator
    {
        using value_type = T;

        FixedBufferAllocator(T *buffer, const usize capacity) : buffer_{buffer}, capacity_{capacity}
        {
        }

        template <usize N>
        explicit FixedBufferAllocator(std::array<T, N> &arr) : buffer_{arr.data()}, capacity_{arr.size()}
        {
        }

        T *allocate(const usize n)
        {
            if (n > capacity_)
                throw std::bad_alloc{};

            return buffer_;
        }

        void deallocate(T *ptr, const usize n) noexcept
        {
            // no-op
        }

      private:
        T *buffer_;
        usize capacity_;
    };
} // namespace retro
