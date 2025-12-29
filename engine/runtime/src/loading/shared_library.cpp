//
// Created by fcors on 12/23/2025.
//
module retro.runtime;

import std;
import sdl;

using namespace retro;

void SharedLibraryBase::load(const std::filesystem::path &path)
{
    unload();

    handle_ = sdl::LoadObject(path.string().c_str());
    if (!handle_)
    {
        throw std::runtime_error{"Failed to LoadLibraryW: " + path.string()};
    }
}

void SharedLibraryBase::unload() noexcept
{
    if (handle_ != nullptr)
    {
        sdl::UnloadObject(handle_);
        handle_ = nullptr;
    }
}
