/**
 * @file file_stream.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

#include <boost/asio.hpp>

module retro.core.io.file_stream;

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
                case FileOpenMode::read_only:
                    return boost::asio::file_base::read_only;
                case FileOpenMode::read_write:
                    return boost::asio::file_base::read_write;
                case FileOpenMode::write_only:
                    return boost::asio::file_base::write_only;
            }
            return boost::asio::file_base::read_only;
        }

        boost::asio::file_base::seek_basis to_seek_basis(const SeekOrigin origin) noexcept
        {
            switch (origin)
            {
                case SeekOrigin::begin:
                    return boost::asio::file_base::seek_set;
                case SeekOrigin::current:
                    return boost::asio::file_base::seek_cur;
                case SeekOrigin::end:
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
                return StreamError::closed;
            }

            if (ec == make_error_code(boost::system::errc::not_supported) ||
                ec == make_error_code(boost::system::errc::operation_not_supported))
            {
                return StreamError::not_supported;
            }

            if (ec == make_error_code(boost::system::errc::invalid_argument))
            {
                return StreamError::invalid_argument;
            }

            if (ec == make_error_code(boost::system::errc::result_out_of_range) ||
                ec == make_error_code(boost::system::errc::value_too_large))
            {
                return StreamError::out_of_range;
            }

            return StreamError::io_error;
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

    StreamResult<std::size_t> FileStream::length() const
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::closed);
        }

        return file_.size();
    }

    StreamResult<std::size_t> FileStream::position() const
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::closed);
        }

        return position_;
    }

    StreamResult<std::size_t> FileStream::seek(const std::size_t offset, const SeekOrigin origin)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::closed);
        }

        boost::system::error_code ec;
        auto result = file_.seek(static_cast<std::int64_t>(offset), to_seek_basis(origin), ec);
        if (ec.failed())
        {
            return std::unexpected(to_stream_error(ec));
        }

        position_ = result;

        return result;
    }

    StreamResult<void> FileStream::set_position(const std::size_t pos)
    {
        EXPECT(seek(pos, SeekOrigin::begin));
        return {};
    }

    StreamResult<std::size_t> FileStream::read(std::span<std::byte> dest)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::closed);
        }

        boost::system::error_code ec;
        auto result = file_.read_some(dest, ec);
        if (ec.failed() && ec != boost::asio::error::eof)
        {
            return std::unexpected(to_stream_error(ec));
        }

        position_ += result;

        return result;
    }

    StreamResult<std::size_t> FileStream::write(std::span<const std::byte> src)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::closed);
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
            return std::unexpected(StreamError::closed);
        }

        // Since we're not using an internal buffer, we don't need to flush
        return {};
    }
} // namespace retro
