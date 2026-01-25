/**
 * @file io.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.core;

namespace retro::filesystem
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

    std::filesystem::path get_executable_path()
    {
        return boost::dll::program_location().parent_path();
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
        std::ios_base::seekdir to_seekdir(const SeekOrigin origin) noexcept
        {
            switch (origin)
            {
                case SeekOrigin::Begin:
                    return std::ios_base::beg;
                case SeekOrigin::Current:
                    return std::ios_base::cur;
                case SeekOrigin::End:
                    return std::ios_base::end;
            }

            return std::ios_base::beg;
        }

        StreamResult<usize> tell_any(std::fstream &f) noexcept
        {
            const auto g = f.tellg();
            if (g != std::streampos(-1))
            {
                return g;
            }

            const auto p = f.tellp();
            if (p != std::streampos(-1))
            {
                return p;
            }

            return std::unexpected(StreamError::IoError);
        }
    } // namespace

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

        auto &f = const_cast<std::fstream &>(file_);

        const auto saved_g = f.tellg();
        const auto saved_p = f.tellp();

        f.clear();
        f.seekg(0, std::ios_base::end);
        const auto end_g = f.tellg();

        f.clear();
        if (saved_g != std::streampos(-1))
        {
            f.seekg(saved_g);
        }
        if (saved_p != std::streampos(-1))
        {
            f.seekp(saved_p);
        }

        if (end_g == std::streampos(-1))
        {
            return std::unexpected(StreamError::IoError);
        }

        return end_g;
    }

    StreamResult<usize> FileStream::position() const
    {
        auto &f = const_cast<std::fstream &>(file_);
        f.clear();
        return tell_any(f);
    }

    StreamResult<usize> FileStream::seek(const usize offset, const SeekOrigin origin)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        file_.clear();

        const auto off = static_cast<std::streamoff>(offset);
        const auto dir = to_seekdir(origin);

        file_.seekg(off, dir);
        file_.seekp(off, dir);

        if (file_.fail() || file_.bad())
        {
            return std::unexpected(StreamError::IoError);
        }

        return tell_any(file_);
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

        file_.clear();

        if (dest.empty())
        {
            return 0;
        }

        file_.read(reinterpret_cast<char *>(dest.data()), static_cast<std::streamsize>(dest.size()));

        const auto got = file_.gcount();

        if (file_.bad())
        {
            return std::unexpected(StreamError::IoError);
        }

        return static_cast<usize>(got);
    }

    StreamResult<usize> FileStream::write(std::span<const std::byte> src)
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        file_.clear();

        if (src.empty())
        {
            return 0;
        }

        file_.write(reinterpret_cast<const char *>(src.data()), static_cast<std::streamsize>(src.size()));

        if (file_.fail() || file_.bad())
        {
            return std::unexpected(StreamError::IoError);
        }

        return src.size();
    }

    StreamResult<void> FileStream::flush()
    {
        if (is_closed())
        {
            return std::unexpected(StreamError::Closed);
        }

        file_.flush();
        if (file_.fail() || file_.bad())
        {
            return std::unexpected(StreamError::IoError);
        }

        return {};
    }
} // namespace retro::filesystem
