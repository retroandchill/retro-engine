//
// Created by fcors on 12/21/2025.
//
module;
#include <SDL3/SDL.h>

#include "retro/core/exports.h"

export module retro.platform:loading.shared_library;

import std;
import retro.core;

namespace retro {
    export enum class LibraryUnloadPolicy {
        UnloadOnDestruction,
        KeepLoaded
    };

    class RETRO_API SharedLibraryBase {
    public:
        SharedLibraryBase() = default;
        inline explicit SharedLibraryBase(SDL_SharedObject* handle) : handle_{handle} {}

        [[nodiscard]] inline bool is_loaded() const noexcept {
            return handle_ != nullptr;
        }

        [[nodiscard]] inline SDL_SharedObject* handle() const noexcept {
            return handle_;
        }

        template <typename Fn>
            requires std::is_function_v<std::remove_pointer_t<Fn>> && std::is_pointer_v<Fn>
        [[nodiscard]] Fn get_function(const CStringView function_name) {
            if (handle_ == nullptr) {
                throw std::runtime_error{"SharedLibrary is not loaded"};
            }

            const auto proc = SDL_LoadFunction(handle_, function_name.data());
            if (proc == nullptr) {
                throw std::runtime_error{"Failed to load function: " + function_name.to_string()};
            }

            return std::bit_cast<Fn>(proc);
        }

        void load(const std::filesystem::path& path);
        void unload() noexcept;

        SDL_SharedObject* handle_{nullptr};
    };

    export template <LibraryUnloadPolicy Policy = LibraryUnloadPolicy::UnloadOnDestruction>
    class SharedLibrary;

    export template <>
    class RETRO_API SharedLibrary<LibraryUnloadPolicy::UnloadOnDestruction> : private SharedLibraryBase {

    public:
        SharedLibrary() = default;

        inline explicit SharedLibrary(const std::filesystem::path& path)
        {
            load(path);
        }

        inline ~SharedLibrary() noexcept
        {
            unload();
        }

        SharedLibrary(const SharedLibrary&)  = delete;
        SharedLibrary& operator=(const SharedLibrary&)  = delete;

        inline SharedLibrary(SharedLibrary&& other) noexcept
            : SharedLibraryBase{other.handle_}
        {
            other.handle_ = nullptr;
        }

        inline SharedLibrary& operator=(SharedLibrary&& other) noexcept
        {
            if (this != &other) {
                unload();
                handle_ = other.handle_;
                other.handle_ = nullptr;
            }
            return *this;
        }

        using SharedLibraryBase::load;
        using SharedLibraryBase::unload;
        using SharedLibraryBase::is_loaded;
        using SharedLibraryBase::handle;
        using SharedLibraryBase::get_function;
    };

    export template <>
    class RETRO_API SharedLibrary<LibraryUnloadPolicy::KeepLoaded> : private SharedLibraryBase {

    public:
        SharedLibrary() = default;

        explicit SharedLibrary(const std::filesystem::path& path)
        {
            load(path);
        }

        using SharedLibraryBase::load;
        using SharedLibraryBase::unload;
        using SharedLibraryBase::is_loaded;
        using SharedLibraryBase::handle;
        using SharedLibraryBase::get_function;
    };
}