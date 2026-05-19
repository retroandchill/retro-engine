/**
 * @file array_buffer_writer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.io.array_buffer_writer;

import std;

namespace retro
{
    export template <std::movable T>
        requires std::is_default_constructible_v<T>
    class ArrayBufferWriter
    {
      public:
        using value_type = T;

        constexpr explicit ArrayBufferWriter(const std::size_t size = 1024)
            : buffer_{std::make_unique<T[]>(size)}, capacity_{size}
        {
        }

        [[nodiscard]] constexpr std::size_t capacity() const noexcept
        {
            return capacity_;
        }

        [[nodiscard]] constexpr std::size_t free_capacity() const noexcept
        {
            return capacity_ - written_count_;
        }

        [[nodiscard]] constexpr std::size_t written_count() const noexcept
        {
            return written_count_;
        }

        [[nodiscard]] std::span<const T> written_span() const noexcept
        {
            return std::span<const T>(buffer_.get(), written_count_);
        }

        constexpr void advance(const std::size_t count)
        {
            if (written_count_ + count > capacity_)
                throw std::invalid_argument{"Attempted to advance past the capacity of the buffer"};

            written_count_ += count;
        }

        constexpr std::span<T> get_span(const std::size_t size_hint) noexcept
        {
            if (const auto min_capacity = written_count_ + size_hint; min_capacity > capacity_)
                grow(std::bit_ceil(min_capacity));

            return std::span<T>(std::next(buffer_.get(), written_count_), free_capacity());
        }

        constexpr void clear() noexcept
        {
            if constexpr (!std::is_trivially_destructible_v<T>)
            {
                for (std::size_t i = 0; i < written_count_; ++i)
                {
                    buffer_[i] = T{};
                }
            }
            written_count_ = 0;
        }

      private:
        void grow(const std::size_t size) noexcept
        {
            auto new_buffer = std::make_unique<T[]>(size);
            std::span<T> new_span{std::next(new_buffer.get(), written_count_), free_capacity()};
            for (std::size_t i = 0; i < written_count_; ++i)
            {
                new_span[i] = std::move(buffer_[i]);
            }
            buffer_ = std::move(new_buffer);
            capacity_ = size;
        }

        void reset_elements() noexcept
        {
            if constexpr (!std::is_trivial_v<T>)
            {
                for (auto span = std::span<T>(buffer_.get(), written_count_); auto &element : span)
                {
                    std::destroy_at(std::addressof(element));
                }
            }
        }

        std::unique_ptr<T[]> buffer_;
        std::size_t written_count_{};
        std::size_t capacity_{};
    };
} // namespace retro
