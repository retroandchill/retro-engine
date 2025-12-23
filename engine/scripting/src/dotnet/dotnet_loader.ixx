//
// Created by fcors on 12/21/2025.
//
module;

#include "retro/core/exports.h"

#include <cassert>
#include <coreclr_delegates.h>
#include <hostfxr.h>

export module retro.scripting.dotnet:loader;

import std;
import retro.core;
import retro.platform.loading;
import :host;

namespace retro::scripting {
    using namespace core;
    using namespace platform;

    export class RETRO_API DotnetLoader {
        using enum LibraryUnloadPolicy;
        constexpr static usize MAX_PATH = 260;

        struct InitializeTag {};
        constexpr static InitializeTag initialize_tag{};

    public:
        explicit DotnetLoader(InitializeTag);

        static inline DotnetLoader& instance() {
            assert(instance_ != nullptr);
            return *instance_;
        }

        static inline void initialize() {
            assert(instance_ == nullptr);
            instance_ = std::make_unique<DotnetLoader>(initialize_tag);
        }

        static inline void shutdown() {
            instance_.reset();
        }

        [[nodiscard]] DotnetHost initialize_for_runtime_config(const std::filesystem::path& path) const;


        // ReSharper disable once CppParameterMayBeConst
        inline int32 close(hostfxr_handle handle) const {
            return close_fptr_(handle);
        }

    private:
        static std::unique_ptr<DotnetLoader> instance_;

        SharedLibrary<KeepLoaded> lib_;
        hostfxr_initialize_for_runtime_config_fn init_fptr_{nullptr};
        hostfxr_get_runtime_delegate_fn get_delegate_fptr_{nullptr};
        hostfxr_close_fn close_fptr_{nullptr};
    };

    export struct DotnetLifecycle {
        inline DotnetLifecycle() {
            DotnetLoader::initialize();
        }

        inline ~DotnetLifecycle() {
            DotnetLoader::shutdown();
        }

        DotnetLifecycle(const DotnetLifecycle&) = delete;
        DotnetLifecycle& operator=(const DotnetLifecycle&) = delete;
        DotnetLifecycle(DotnetLifecycle&&) noexcept = delete;
        DotnetLifecycle& operator=(DotnetLifecycle&&) noexcept = delete;
    };
}