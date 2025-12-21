//
// Created by fcors on 12/21/2025.
//
module;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <retro/core/exports.h>
#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>

export module retro.scripting.dotnet;

import std;
import retro.platform.loading;

namespace retro::scripting {
    using namespace platform;

    class Hostfxr {
    public:
        Hostfxr()
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

        Hostfxr(const Hostfxr&) = delete;
        Hostfxr(Hostfxr&& other) noexcept : lib_(std::move(other.lib_)), init_fptr_(other.init_fptr_), get_delegate_fptr_(other.get_delegate_fptr_), close_fptr_(other.close_fptr_) {
            other.init_fptr_ = nullptr;
            other.get_delegate_fptr_ = nullptr;
            other.close_fptr_ = nullptr;
        }


        ~Hostfxr() {
            if (lib_.is_loaded() && close_fptr_ != nullptr) {
                close_fptr_(lib_.handle());
            }
        }

        Hostfxr& operator=(const Hostfxr&) = delete;

        Hostfxr& operator=(Hostfxr&& other) noexcept {
            lib_ = std::move(other.lib_);
            init_fptr_ = other.init_fptr_;
            get_delegate_fptr_ = other.get_delegate_fptr_;
            close_fptr_ = other.close_fptr_;
            other.init_fptr_ = nullptr;
            other.get_delegate_fptr_ = nullptr;
            other.close_fptr_ = nullptr;
            return *this;
        }

    private:
        SharedLibrary lib_;
        hostfxr_initialize_for_runtime_config_fn init_fptr_{nullptr};
        hostfxr_get_runtime_delegate_fn get_delegate_fptr_{nullptr};
        hostfxr_close_fn close_fptr_{nullptr};
    };
}