//
// Created by fcors on 12/21/2025.
//
module;

#include "retro/core/exports.h"
#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>

export module retro.scripting.dotnet;

import std;
import retro.core;
import retro.platform.loading;

namespace retro::scripting {
    using namespace core;
    using namespace platform;

    export class RETRO_API DotnetHost {
        constexpr static usize MAX_PATH = 260;

    public:
        DotnetHost()
        {
            // 1. Locate hostfxr.dll via nethost
            std::array<char_t, MAX_PATH> path;
            size_t path_size = std::size(path);
            if (get_hostfxr_path(path.data(), &path_size, nullptr) != 0) {
                throw std::runtime_error{"get_hostfxr_path failed to locate hostfxr.dll"};
            }

            // 2. Load the DLL
            std::filesystem::path dll_path{path.data()};
            lib_ = SharedLibrary{dll_path};
            if (!lib_.is_loaded()) {
                throw std::runtime_error{"Failed to LoadLibraryW(hostfxr.dll)"};
            }

            // 3. Resolve the functions we need
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

        DotnetHost(const DotnetHost&) = delete;
        DotnetHost(DotnetHost&& other) noexcept = delete;
        ~DotnetHost() = default;
        DotnetHost& operator=(const DotnetHost&) = delete;
        DotnetHost& operator=(DotnetHost&& other) noexcept = delete;

    private:
        SharedLibrary lib_;
        hostfxr_initialize_for_runtime_config_fn init_fptr_{nullptr};
        hostfxr_get_runtime_delegate_fn get_delegate_fptr_{nullptr};
        hostfxr_close_fn close_fptr_{nullptr};

        hostfxr_handle handle_{nullptr};
    };
}