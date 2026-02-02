/**
 * @file buffered_stream.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.core.io.buffered_stream;

namespace retro
{
    StreamResult<std::span<const std::byte>> BufferedStream::peek(std::size_t count)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        if (!can_read())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        std::size_t buffer_offset = position_ - buffer_start_;
        std::size_t available_in_buffer = buffer_end_ - buffer_start_;
        std::size_t data_available = available_in_buffer - buffer_offset;

        if (data_available < count)
        {
            EXPECT(fill_buffer(count - data_available));
            buffer_offset = position_ - buffer_start_;
            available_in_buffer = buffer_end_ - buffer_start_;
            data_available = available_in_buffer - buffer_offset;
        }

        const std::size_t to_return = std::min(count, data_available);
        return std::span<const std::byte>{std::next(buffer_.data(), static_cast<std::ptrdiff_t>(buffer_offset)),
                                          to_return};
    }

    bool BufferedStream::can_read() const
    {
        return inner_->can_read();
    }

    bool BufferedStream::can_write() const
    {
        return inner_->can_write();
    }

    bool BufferedStream::can_seek() const
    {
        return inner_->can_seek();
    }

    bool BufferedStream::is_closed() const
    {
        return inner_->is_closed();
    }

    void BufferedStream::close() noexcept
    {
        inner_->close();
        buffer_.clear();
    }

    StreamResult<std::size_t> BufferedStream::length() const
    {
        return inner_->length();
    }

    StreamResult<std::size_t> BufferedStream::position() const
    {
        return position_;
    }

    StreamResult<std::size_t> BufferedStream::seek(std::size_t offset, SeekOrigin origin)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        if (!can_seek())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        EXPECT(inner_->seek(offset, origin));
        EXPECT_ASSIGN(position_, inner_->position());
        buffer_start_ = position_;
        buffer_end_ = position_;

        return position_;
    }

    StreamResult<void> BufferedStream::set_position(std::size_t pos)
    {
        EXPECT(seek(pos, SeekOrigin::Begin));
        return {};
    }

    StreamResult<std::size_t> BufferedStream::read(std::span<std::byte> dest)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        if (!can_read())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        std::size_t bytes_read = 0;

        while (bytes_read < dest.size())
        {
            std::size_t buffer_offset = position_ - buffer_start_;
            std::size_t available_in_buffer = buffer_end_ - buffer_start_;

            if (buffer_offset >= available_in_buffer)
            {
                if (available_in_buffer == buffer_.size())
                {
                    buffer_start_ = position_;
                    buffer_end_ = position_;
                }

                EXPECT(fill_buffer(dest.size() - bytes_read));

                buffer_offset = position_ - buffer_start_;
                available_in_buffer = buffer_end_ - buffer_start_;

                if (buffer_offset >= available_in_buffer)
                {
                    break;
                }
            }

            std::size_t to_copy = std::min(dest.size() - bytes_read, available_in_buffer - buffer_offset);
            std::memcpy(dest.data() + bytes_read, buffer_.data() + buffer_offset, to_copy);

            bytes_read += to_copy;
            position_ += to_copy;
        }

        return bytes_read;
    }

    StreamResult<std::size_t> BufferedStream::write(std::span<const std::byte> src)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        if (!can_write())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        auto result = inner_->write(src);
        if (result)
        {
            position_ += result.value();

            buffer_start_ = position_;
            buffer_end_ = position_;
        }

        return result;
    }

    StreamResult<void> BufferedStream::flush()
    {
        return inner_->flush();
    }

    StreamResult<void> BufferedStream::fill_buffer(const std::size_t min_required)
    {
        if (buffer_start_ > 0)
        {
            const std::size_t existing_data = buffer_end_ - buffer_start_;
            if (existing_data > 0)
            {
                std::memmove(buffer_.data(), buffer_.data() + buffer_start_, existing_data);
            }
            buffer_start_ += existing_data;
            buffer_end_ = buffer_start_;
        }

        while (true)
        {
            std::size_t current_available = buffer_end_ - buffer_start_;
            std::size_t buffer_offset = position_ - buffer_start_;
            std::size_t data_available = current_available - buffer_offset;

            // Check if we have enough data now
            if (data_available >= min_required)
            {
                return {};
            }

            const std::size_t space_available = buffer_.size() - (buffer_end_ - buffer_start_);
            if (space_available == 0)
            {
                return {};
            }

            auto result = inner_->read(std::span{buffer_.data() + (buffer_end_ - buffer_start_), space_available});
            if (!result)
            {
                return std::unexpected(result.error());
            }

            if (result.value() == 0)
            {
                return {};
            }

            buffer_end_ += result.value();
        }
    }
} // namespace retro
