/**
 * @file stream.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.io.stream;

import std;

namespace retro
{
    export enum class SeekOrigin
    {
        Begin,
        Current,
        End
    };

    export enum class StreamError
    {
        Closed,
        NotSupported,
        InvalidArgument,
        OutOfRange,
        IoError
    };

    export template <typename T>
    using StreamResult = std::expected<T, StreamError>;

    export class RETRO_API Stream
    {
      public:
        virtual ~Stream() = default;

        [[nodiscard]] virtual bool can_read() const = 0;
        [[nodiscard]] virtual bool can_write() const = 0;
        [[nodiscard]] virtual bool can_seek() const = 0;

        [[nodiscard]] virtual bool is_closed() const = 0;
        virtual void close() noexcept = 0;

        [[nodiscard]] virtual StreamResult<std::size_t> length() const = 0;
        [[nodiscard]] virtual StreamResult<std::size_t> position() const = 0;
        [[nodiscard]] virtual StreamResult<std::size_t> seek(std::size_t offset, SeekOrigin origin) = 0;
        [[nodiscard]] virtual StreamResult<void> set_position(std::size_t pos) = 0;

        [[nodiscard]] virtual StreamResult<std::size_t> read(std::span<std::byte> dest) = 0;
        [[nodiscard]] virtual StreamResult<std::size_t> write(std::span<const std::byte> src) = 0;
        [[nodiscard]] virtual StreamResult<void> flush() = 0;

        [[nodiscard]] virtual StreamResult<std::int32_t> read_byte();
        [[nodiscard]] virtual StreamResult<void> write_byte(std::byte byte);

        [[nodiscard]] StreamResult<std::vector<std::byte>> read_all();

      private:
        [[nodiscard]] StreamResult<std::vector<std::byte>> read_all_with_length(std::size_t len);
        [[nodiscard]] StreamResult<std::vector<std::byte>> read_bytes_chunked();
    };
} // namespace retro
