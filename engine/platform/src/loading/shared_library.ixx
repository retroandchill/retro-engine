//
// Created by fcors on 12/21/2025.
//
module;
#include <SDL3/SDL.h>

#include "retro/core/exports.h"

export module retro.platform.loading:shared_library;

import std;
import retro.core.strings;

namespace retro::platform {
    using namespace core;

    export class RETRO_API SharedLibrary {

    public:
        SharedLibrary() = default;

        explicit SharedLibrary(const std::filesystem::path& path)
        {
            load(path);
        }

        ~SharedLibrary() noexcept
        {
            unload();
        }

        SharedLibrary(const SharedLibrary&) = delete;
        SharedLibrary& operator=(const SharedLibrary&) = delete;

        SharedLibrary(SharedLibrary&& other) noexcept
            : handle_{other.handle_}
        {
            other.handle_ = nullptr;
        }

        SharedLibrary& operator=(SharedLibrary&& other) noexcept
        {
            if (this != &other) {
                unload();
                handle_ = other.handle_;
                other.handle_ = nullptr;
            }
            return *this;
        }

        void load(const std::filesystem::path& path)
        {
            unload();

            handle_ = SDL_LoadObject(path.string().c_str());
            if (!handle_) {
                throw std::runtime_error{"Failed to LoadLibraryW: " + path.string()};
            }
        }

        // NOLINTNEXTLINE
        void unload() noexcept {
            if (handle_ != nullptr) {
                SDL_UnloadObject(handle_);
                handle_ = nullptr;
            }
        }

        [[nodiscard]] bool is_loaded() const noexcept {
            return handle_ != nullptr;
        }

        [[nodiscard]] SDL_SharedObject* handle() const noexcept {
            return handle_;
        }

        template <typename Fn>
            requires std::is_function_v<std::remove_pointer_t<Fn>> && std::is_pointer_v<Fn>
        [[nodiscard]] Fn get_function(const CStringView function_name) {
            if (handle_ == nullptr) {
                throw std::runtime_error{"SharedLibrary is not loaded"};
            }

            auto proc = SDL_LoadFunction(handle_, function_name.data());
            if (proc == nullptr) {
                throw std::runtime_error{"Failed to load function: " + function_name.to_string()};
            }

            return std::bit_cast<Fn>(proc);
        }

    private:
        SDL_SharedObject* handle_{nullptr};
    };
}