/**
 * @file buffered_stream.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.io.buffered_stream;

import std;
import retro.core.io.stream;

namespace retro
{
    /**
     * Stream that reads bytes into a buffer to enable peaking ahead without actually advacing the stream.
     * @remarks This type does not own the underlying Stream, so it is up to the user to ensure that this type does not
     * outlive the underlying stream.
     */
    export class RETRO_API BufferedStream final : public Stream
    {
        static constexpr std::size_t default_buffer_size = 8192;

      public:
        explicit inline BufferedStream(Stream &underlying, std::size_t buffer_size = default_buffer_size)
            : inner_{&underlying}, buffer_{buffer_size}
        {
        }

        StreamResult<std::span<const std::byte>> peek(std::size_t count);

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
        StreamResult<void> fill_buffer(std::size_t min_required);

        Stream *inner_;
        std::vector<std::byte> buffer_;
        std::size_t buffer_start_{0};
        std::size_t buffer_end_{0};
        std::size_t position_{0};
    };
} // namespace retro
