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

        template <auto Functor, typename... Args>
            requires std::invocable<decltype(Functor), Args..., boost::system::error_code>
        StreamResult<std::invoke_result_t<decltype(Functor), Args..., boost::system::error_code>> evaluate_result(
            Args &&...args)
        {
            boost::system::error_code ec;
            auto result = Functor(std::forward<Args>(args)..., ec);
            if (ec.failed())
            {
                return std::unexpected(to_stream_error(ec));
            }

            return std::move(result);
        }
    } // namespace

    FileStream::FileStream(PrivateInit, FileHandle handle) : file_{std::move(handle)}
    {
    }

    StreamResult<std::unique_ptr<FileStream>> FileStream::open(const std::filesystem::path &path, FileOpenMode mode)
    {
        boost::asio::basic_stream_file file{global_io_context().get_executor()};

        boost::system::error_code ec;
        file.open(path.string(), to_open_flags(mode), ec);
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
        auto result = file_.seek(offset, to_seek_basis(origin), ec);
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
} // namespace retro
