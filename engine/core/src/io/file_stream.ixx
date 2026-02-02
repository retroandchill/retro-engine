/**
 * @file file_stream.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <boost/asio.hpp>

export module retro.core.io.file_stream;

import std;
import retro.core.io.stream;

namespace retro
{
    export RETRO_API std::vector<std::byte> read_binary_file(const std::filesystem::path &path);

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

        [[nodiscard]] StreamResult<std::size_t> length() const override;

        [[nodiscard]] StreamResult<std::size_t> position() const override;

        [[nodiscard]] StreamResult<std::size_t> seek(std::size_t offset, SeekOrigin origin) override;

        [[nodiscard]] StreamResult<void> set_position(std::size_t pos) override;

        [[nodiscard]] StreamResult<std::size_t> read(std::span<std::byte> dest) override;

        [[nodiscard]] StreamResult<std::size_t> write(std::span<const std::byte> src) override;

        [[nodiscard]] StreamResult<void> flush() override;

      private:
        FileHandle file_;
        std::size_t position_{0};
    };
} // namespace retro
