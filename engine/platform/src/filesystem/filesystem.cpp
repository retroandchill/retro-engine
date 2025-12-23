//
// Created by fcors on 12/23/2025.
//
module;

#include <SDL3/SDL_filesystem.h>

module retro.platform.filesystem;

namespace retro::platform::filesystem {
    std::filesystem::path get_executable_path() {
        auto *basePath = SDL_GetBasePath();
        return std::filesystem::path{basePath};
    }
}
