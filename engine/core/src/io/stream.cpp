/**
 * @file stream.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.core.io.stream;

namespace retro
{
    StreamResult<std::int32_t> Stream::read_byte()
    {
        std::array<std::byte, 1> buffer{};
        EXPECT_ASSIGN(auto bytes_read, read(buffer))
        if (bytes_read == 0)
        {
            return -1;
        }

        return static_cast<std::int32_t>(buffer[0]);
    }

    StreamResult<void> Stream::write_byte(const std::byte byte)
    {
        std::array buffer{byte};
        EXPECT(write(buffer));
        return {};
    }

    StreamResult<std::vector<std::byte>> Stream::read_all()
    {
        if (!can_read())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        return length()
            .and_then([this](const std::size_t num_bytes) -> StreamResult<std::vector<std::byte>>
                      { return read_all_with_length(num_bytes); })
            .or_else([this](StreamError) { return read_bytes_chunked(); });
    }

    StreamResult<std::vector<std::byte>> Stream::read_all_with_length(const std::size_t len)
    {
        std::vector<std::byte> buffer(len);
        EXPECT(read(buffer));
        return std::move(buffer);
    }

    StreamResult<std::vector<std::byte>> Stream::read_bytes_chunked()
    {
        constexpr std::size_t BUFFER_SIZE = 4096;
        std::array<std::byte, BUFFER_SIZE> buffer{};
        std::vector<std::byte> result;
        std::size_t bytes_read;
        do
        {
            EXPECT_ASSIGN(bytes_read, read(buffer));
            result.insert(result.end(),
                          buffer.begin(),
                          std::next(buffer.begin(), static_cast<std::ptrdiff_t>(bytes_read)));
        } while (bytes_read > 0);

        return std::move(result);
    }
} // namespace retro
