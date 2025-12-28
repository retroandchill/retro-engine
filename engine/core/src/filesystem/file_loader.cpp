//
// Created by fcors on 12/28/2025.
//
module retro.core;

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
        file.read(std::bit_cast<char *>(data.data()), static_cast<std::streamsize>(size));
        return data;
    }
} // namespace retro
