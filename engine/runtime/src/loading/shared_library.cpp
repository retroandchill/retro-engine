//
// Created by fcors on 12/23/2025.
//
module;

#include <SDL3/SDL.h>

module retro.runtime;

import std;

using namespace retro;

void SharedLibraryBase::load(const std::filesystem::path &path)
{
    unload();

    handle_ = SDL_LoadObject(path.string().c_str());
    if (!handle_)
    {
        throw std::runtime_error{"Failed to LoadLibraryW: " + path.string()};
    }
}

void SharedLibraryBase::unload() noexcept
{
    if (handle_ != nullptr)
    {
        SDL_UnloadObject(handle_);
        handle_ = nullptr;
    }
}
