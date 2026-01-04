/**
 * @file dotnet_loader.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <nethost.h>

module retro.scripting;

using namespace retro;

DotnetInitializationHandle DotnetLoader::initialize_for_runtime_config(const std::filesystem::path &path) const
{
    hostfxr_handle handle;
    if (init_fptr_(path.c_str(), nullptr, &handle) != 0)
    {
        throw std::runtime_error{"hostfxr_initialize_for_runtime_config failed"};
    }

    return DotnetInitializationHandle{handle, close_fptr_};
}

DotnetLoader::DotnetLoader()
{
    std::array<char_t, MAX_PATH> path;
    size_t path_size = std::size(path);
    if (get_hostfxr_path(path.data(), &path_size, nullptr) != 0)
    {
        throw std::runtime_error{"get_hostfxr_path failed to locate hostfxr.dll"};
    }

    std::filesystem::path dll_path{path.data()};
    lib_ = boost::dll::shared_library{dll_path};
    if (!lib_.is_loaded())
    {
        throw std::runtime_error{"Failed to LoadLibraryW(hostfxr.dll)"};
    }

    init_fptr_ = lib_.get<InitFn>("hostfxr_initialize_for_runtime_config");
    get_delegate_fptr_ = lib_.get<GetDelegateFn>("hostfxr_get_runtime_delegate");
    close_fptr_ = lib_.get<CloseFn>("hostfxr_close");

    if (init_fptr_ == nullptr || get_delegate_fptr_ == nullptr || close_fptr_ == nullptr)
    {
        lib_.unload();
        throw std::runtime_error{
            "Failed to resolve hostfxr_initialize_for_runtime_config, hostfxr_get_runtime_delegate, or hostfxr_close"};
    }
}
