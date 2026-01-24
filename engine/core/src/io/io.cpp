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
} // namespace retro::filesystem
