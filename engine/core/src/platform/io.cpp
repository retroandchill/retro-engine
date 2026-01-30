/**
 * @file io.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

#include <boost/asio.hpp>

module retro.core;

import std;

namespace retro
{
    std::vector<std::byte> read_binary_file(const std::filesystem::path &path)
    {
        std::ifstream file{path, std::ios::binary | std::ios::ate};
        if (!file)
            throw std::runtime_error{"Failed to open shader file"};

        const auto size = static_cast<size_t>(file.tellg());
        std::vector<std::byte> data(size);
        file.seekg(0);
        file.read(reinterpret_cast<char *>(data.data()), static_cast<std::streamsize>(size));
        return data;
    }

    StreamResult<int32> Stream::read_byte()
    {
        std::array<std::byte, 1> buffer{};
        EXPECT_ASSIGN(auto bytes_read, read(buffer))
        if (bytes_read == 0)
        {
            return -1;
        }

        return static_cast<int32>(buffer[0]);
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
            .and_then([this](const usize num_bytes) -> StreamResult<std::vector<std::byte>>
                      { return read_all_with_length(num_bytes); })
            .or_else([this](StreamError) { return read_bytes_chunked(); });
    }

    StreamResult<std::vector<std::byte>> Stream::read_all_with_length(const usize len)
    {
        std::vector<std::byte> buffer(len);
        EXPECT(read(buffer));
        return std::move(buffer);
    }

    StreamResult<std::vector<std::byte>> Stream::read_bytes_chunked()
    {
        constexpr usize BUFFER_SIZE = 4096;
        std::array<std::byte, BUFFER_SIZE> buffer{};
        std::vector<std::byte> result;
        usize bytes_read;
        do
        {
            EXPECT_ASSIGN(bytes_read, read(buffer));
            result.insert(result.end(), buffer.begin(), std::next(buffer.begin(), static_cast<isize>(bytes_read)));
        } while (bytes_read > 0);

        return std::move(result);
    }

    namespace
    {
        boost::asio::io_context &global_io_context()
        {
            static boost::asio::io_context ctx{};
            return ctx;
        }

        boost::asio::file_base::flags to_open_flags(const FileOpenMode mode) noexcept
        {
            switch (mode)
            {
                case FileOpenMode::ReadOnly:
                    return boost::asio::file_base::read_only;
                case FileOpenMode::ReadWrite:
                    return boost::asio::file_base::read_write;
                case FileOpenMode::WriteOnly:
                    return boost::asio::file_base::write_only;
            }
            return boost::asio::file_base::read_only;
        }

        boost::asio::file_base::seek_basis to_seek_basis(const SeekOrigin origin) noexcept
        {
            switch (origin)
            {
                case SeekOrigin::Begin:
                    return boost::asio::file_base::seek_set;
                case SeekOrigin::Current:
                    return boost::asio::file_base::seek_cur;
                case SeekOrigin::End:
                    return boost::asio::file_base::seek_end;
            }

            return boost::asio::file_base::seek_set;
        }

        StreamError to_stream_error(const boost::system::error_code &ec) noexcept
        {
            // Asio often reports file-handle issues as "bad_descriptor".
            if (ec == boost::asio::error::bad_descriptor ||
                ec == make_error_code(boost::system::errc::bad_file_descriptor))
            {
                return StreamError::Closed;
            }

            if (ec == make_error_code(boost::system::errc::not_supported) ||
                ec == make_error_code(boost::system::errc::operation_not_supported))
            {
                return StreamError::NotSupported;
            }

            if (ec == make_error_code(boost::system::errc::invalid_argument))
            {
                return StreamError::InvalidArgument;
            }

            if (ec == make_error_code(boost::system::errc::result_out_of_range) ||
                ec == make_error_code(boost::system::errc::value_too_large))
            {
                return StreamError::OutOfRange;
            }

            return StreamError::IoError;
        }
    } // namespace

    FileStream::FileStream(PrivateInit, FileHandle handle) : file_{std::move(handle)}
    {
    }

    StreamResult<std::unique_ptr<FileStream>> FileStream::open(const std::filesystem::path &path,
                                                               const FileOpenMode mode)
    {
        boost::asio::basic_stream_file file{global_io_context().get_executor()};

        boost::system::error_code ec;
        std::ignore = file.open(path.string(), to_open_flags(mode), ec);
        if (ec.failed())
        {
            return std::unexpected(to_stream_error(ec));
        }

        return std::make_unique<FileStream>(PrivateInit{}, std::move(file));
    }

    bool FileStream::can_read() const
    {
        return true;
    }

    bool FileStream::can_write() const
    {
        return true;
    }

    bool FileStream::can_seek() const
    {
        return true;
    }

    bool FileStream::is_closed() const
    {
        return !file_.is_open();
    }

    void FileStream::close() noexcept
    {
        file_.close();
    }

    StreamResult<usize> FileStream::length() const
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        return file_.size();
    }

    StreamResult<usize> FileStream::position() const
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        return position_;
    }

    StreamResult<usize> FileStream::seek(const usize offset, const SeekOrigin origin)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        boost::system::error_code ec;
        auto result = file_.seek(static_cast<int64>(offset), to_seek_basis(origin), ec);
        if (ec.failed())
        {
            return std::unexpected(to_stream_error(ec));
        }

        position_ = result;

        return result;
    }

    StreamResult<void> FileStream::set_position(const usize pos)
    {
        EXPECT(seek(pos, SeekOrigin::Begin));
        return {};
    }

    StreamResult<usize> FileStream::read(std::span<std::byte> dest)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        boost::system::error_code ec;
        auto result = file_.read_some(dest, ec);
        if (ec.failed())
        {
            return std::unexpected(to_stream_error(ec));
        }

        position_ = result;

        return result;
    }

    StreamResult<usize> FileStream::write(std::span<const std::byte> src)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        boost::system::error_code ec;
        auto result = file_.write_some(src, ec);
        if (ec.failed())
        {
            return std::unexpected(to_stream_error(ec));
        }

        position_ += result;

        return result;
    }

    StreamResult<void> FileStream::flush()
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        // Since we're not using an internal buffer, we don't need to flush
        return {};
    }

    StreamResult<std::span<const std::byte>> BufferedStream::peek(usize count)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        if (!can_read())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        usize buffer_offset = position_ - buffer_start_;
        usize available_in_buffer = buffer_end_ - buffer_start_;
        usize data_available = available_in_buffer - buffer_offset;

        if (data_available < count)
        {
            EXPECT(fill_buffer(count - data_available));
            buffer_offset = position_ - buffer_start_;
            available_in_buffer = buffer_end_ - buffer_start_;
            data_available = available_in_buffer - buffer_offset;
        }

        const usize to_return = std::min(count, data_available);
        return std::span<const std::byte>{std::next(buffer_.data(), static_cast<isize>(buffer_offset)), to_return};
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

    StreamResult<usize> BufferedStream::length() const
    {
        return inner_->length();
    }

    StreamResult<usize> BufferedStream::position() const
    {
        return position_;
    }

    StreamResult<usize> BufferedStream::seek(usize offset, SeekOrigin origin)
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

    StreamResult<void> BufferedStream::set_position(usize pos)
    {
        EXPECT(seek(pos, SeekOrigin::Begin));
        return {};
    }

    StreamResult<usize> BufferedStream::read(std::span<std::byte> dest)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        if (!can_read())
        {
            return std::unexpected(StreamError::NotSupported);
        }

        usize bytes_read = 0;

        while (bytes_read < dest.size())
        {
            usize buffer_offset = position_ - buffer_start_;
            usize available_in_buffer = buffer_end_ - buffer_start_;

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

            usize to_copy = std::min(dest.size() - bytes_read, available_in_buffer - buffer_offset);
            std::memcpy(dest.data() + bytes_read, buffer_.data() + buffer_offset, to_copy);

            bytes_read += to_copy;
            position_ += to_copy;
        }

        return bytes_read;
    }

    StreamResult<usize> BufferedStream::write(std::span<const std::byte> src)
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

    StreamResult<void> BufferedStream::fill_buffer(const usize min_required)
    {
        if (buffer_start_ > 0 && buffer_end_ < buffer_.size())
        {
            const usize existing_data = buffer_end_ - buffer_start_;
            std::memcpy(buffer_.data(), buffer_.data() + buffer_start_, existing_data);
            buffer_start_ = 0;
            buffer_end_ = existing_data;
        }

        while (true)
        {
            usize current_available = buffer_end_ - buffer_start_;
            usize buffer_offset = position_ - buffer_start_;
            usize data_available = current_available - buffer_offset;

            // Check if we have enough data now
            if (data_available >= min_required)
            {
                return {};
            }

            const usize space_available = buffer_.size() - buffer_end_;
            if (space_available == 0)
            {
                return {};
            }

            auto result = inner_->read(std::span{buffer_.data() + buffer_end_, space_available});
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
