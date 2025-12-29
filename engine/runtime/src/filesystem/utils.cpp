//
// Created by fcors on 12/23/2025.
//
module retro.runtime;

import sdl;

namespace retro::filesystem
{
    std::filesystem::path get_executable_path()
    {
        auto *basePath = sdl::GetBasePath();
        return std::filesystem::path{basePath};
    }
} // namespace retro::filesystem
