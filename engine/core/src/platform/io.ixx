/**
 * @file io.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <boost/asio.hpp>

export module retro.core:io;

import std;
import :concepts;
import :defines;

namespace retro
{
    export RETRO_API std::vector<std::byte> read_binary_file(const std::filesystem::path &path);

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

        [[nodiscard]] virtual StreamResult<usize> length() const = 0;
        [[nodiscard]] virtual StreamResult<usize> position() const = 0;
        [[nodiscard]] virtual StreamResult<usize> seek(usize offset, SeekOrigin origin) = 0;
        [[nodiscard]] virtual StreamResult<void> set_position(usize pos) = 0;

        [[nodiscard]] virtual StreamResult<usize> read(std::span<std::byte> dest) = 0;
        [[nodiscard]] virtual StreamResult<usize> write(std::span<const std::byte> src) = 0;
        [[nodiscard]] virtual StreamResult<void> flush() = 0;

        [[nodiscard]] virtual StreamResult<int32> read_byte();
        [[nodiscard]] virtual StreamResult<void> write_byte(std::byte byte);

        [[nodiscard]] StreamResult<std::vector<std::byte>> read_all();

      private:
        [[nodiscard]] StreamResult<std::vector<std::byte>> read_all_with_length(usize len);
        [[nodiscard]] StreamResult<std::vector<std::byte>> read_bytes_chunked();
    };

    export enum class FileOpenMode
    {
        ReadOnly,
        ReadWrite,
        WriteOnly
    };

    export class RETRO_API FileStream final : public Stream
    {
        using FileHandle = boost::asio::basic_stream_file<boost::asio::io_context::executor_type>;

        struct PrivateInit
        {
        };

      public:
        explicit FileStream(PrivateInit, FileHandle handle);

        static StreamResult<std::unique_ptr<FileStream>> open(const std::filesystem::path &path, FileOpenMode mode);

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

    /**
     * Stream that reads bytes into a buffer to enable peaking ahead without actually advacing the stream.
     * @remarks This type does not own the underlying Stream, so it is up to the user to ensure that this type does not
     * outlive the underlying stream.
     */
    export class RETRO_API BufferedStream final : public Stream
    {
        static constexpr usize DEFAULT_BUFFER_SIZE = 8192;

      public:
        explicit inline BufferedStream(Stream &underlying, usize buffer_size = DEFAULT_BUFFER_SIZE)
            : inner_{&underlying}, buffer_{buffer_size}
        {
        }

        StreamResult<std::span<const std::byte>> peek(usize count);

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
        StreamResult<void> fill_buffer(usize min_required);

        Stream *inner_;
        std::vector<std::byte> buffer_;
        usize buffer_start_{0};
        usize buffer_end_{0};
        usize position_{0};
    };
} // namespace retro
