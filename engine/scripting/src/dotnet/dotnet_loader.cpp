//
// Created by fcors on 12/23/2025.
//
module;

#include <nethost.h>

module retro.scripting.dotnet;

using namespace retro;

DotnetInitializationHandle DotnetLoader::initialize_for_runtime_config(const std::filesystem::path &path) const {
    hostfxr_handle handle;
    if (init_fptr_(path.c_str(), nullptr, &handle) != 0) {
        throw std::runtime_error{"hostfxr_initialize_for_runtime_config failed"};
    }

    return DotnetInitializationHandle{handle, close_fptr_};
}

DotnetLoader::DotnetLoader() {
    using enum LibraryUnloadPolicy;

    std::array<char_t, MAX_PATH> path;
    size_t path_size = std::size(path);
    if (get_hostfxr_path(path.data(), &path_size, nullptr) != 0) {
        throw std::runtime_error{"get_hostfxr_path failed to locate hostfxr.dll"};
    }

    std::filesystem::path dll_path{path.data()};
    lib_ = SharedLibrary<KeepLoaded>{dll_path};
    if (!lib_.is_loaded()) {
        throw std::runtime_error{"Failed to LoadLibraryW(hostfxr.dll)"};
    }

    init_fptr_ = lib_.get_function<hostfxr_initialize_for_runtime_config_fn>("hostfxr_initialize_for_runtime_config");
    get_delegate_fptr_ = lib_.get_function<hostfxr_get_runtime_delegate_fn>("hostfxr_get_runtime_delegate");
    close_fptr_ = lib_.get_function<hostfxr_close_fn>("hostfxr_close");

    if (init_fptr_ == nullptr || get_delegate_fptr_ == nullptr || close_fptr_ == nullptr) {
        lib_.unload();
        throw std::runtime_error{
            "Failed to resolve hostfxr_initialize_for_runtime_config, hostfxr_get_runtime_delegate, or hostfxr_close"
        };
    }
}