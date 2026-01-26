/**
 * @file io.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core:io;

import std;
import boost;
import :defines;

namespace retro::filesystem
{
    export RETRO_API std::vector<std::byte> read_binary_file(const std::filesystem::path &path);

    export [[nodiscard]] RETRO_API std::filesystem::path get_executable_path();

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

    export class RETRO_API Stream : boost::noncopyable
    {
      public:
        virtual ~Stream() = default;

        [[nodiscard]] virtual bool can_read() const = 0;
        [[nodiscard]] virtual bool can_write() const = 0;
        [[nodiscard]] virtual bool can_seek() const = 0;

        [[nodiscard]] virtual bool is_closed() const = 0;
        virtual void close() noexcept = 0;

        [[nodiscard]] virtual StreamResult<usize> length() const = 0;
        [[nodiscard]] virtual StreamResult<usize> position() const = 0;
        [[nodiscard]] virtual StreamResult<usize> seek(usize offset, SeekOrigin origin) = 0;
        [[nodiscard]] virtual StreamResult<void> set_position(usize pos) = 0;

        [[nodiscard]] virtual StreamResult<usize> read(std::span<std::byte> dest) = 0;
        [[nodiscard]] virtual StreamResult<usize> write(std::span<const std::byte> src) = 0;
        [[nodiscard]] virtual StreamResult<void> flush() = 0;

        [[nodiscard]] virtual StreamResult<int32> read_byte();
        [[nodiscard]] virtual StreamResult<void> write_byte(std::byte byte);
    };

    export class RETRO_API FileStream final : public Stream
    {
        using FileHandle = boost::asio::stream_file;

      public:
        explicit FileStream(FileHandle handle);

        [[nodiscard]] bool can_read() const override;

        [[nodiscard]] bool can_write() const override;

        [[nodiscard]] bool can_seek() const override;

        [[nodiscard]] bool is_closed() const override;

        void close() noexcept override;

        [[nodiscard]] StreamResult<usize> length() const override;

        [[nodiscard]] StreamResult<usize> position() const override;

        [[nodiscard]] StreamResult<usize> seek(usize offset, SeekOrigin origin) override;

        [[nodiscard]] StreamResult<void> set_position(usize pos) override;

        [[nodiscard]] StreamResult<usize> read(std::span<std::byte> dest) override;

        [[nodiscard]] StreamResult<usize> write(std::span<const std::byte> src) override;

        [[nodiscard]] StreamResult<void> flush() override;

      private:
        FileHandle file_;
        usize position_{0};
    };
} // namespace retro::filesystem
