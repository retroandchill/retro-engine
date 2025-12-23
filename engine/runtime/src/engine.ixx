//
// Created by fcors on 12/21/2025.
//
module;

#include "retro/core/exports.h"
#include <cassert>

export module retro.runtime.engine;

import std;
import retro.core;
import retro.core.strings;
import retro.platform;
import retro.platform.window;
import retro.scripting.dotnet;

namespace retro::runtime {
    using namespace core;
    using namespace platform;
    using namespace scripting;

    export class RETRO_API Engine {
        struct InitializeTag {};
        constexpr static InitializeTag initialize_tag{};

    public:
        inline Engine(InitializeTag, const CStringView name, const int32 width, const int32 height) : window_{platform_, width, height, name} {}

        static inline Engine& instance() {
            assert(instance_ != nullptr);
            return *instance_;
        }

        inline static void initialize(const CStringView name, const int32 width, const int32 height) {
            assert(instance_ == nullptr);
            instance_ = std::make_unique<Engine>(initialize_tag, name, width, height);
        }

        inline static void shutdown() {
            instance_.reset();
        }

        void run();

    private:
        void tick(float delta_time);

        void render();

        static std::unique_ptr<Engine> instance_;

        Platform platform_;
        Window window_;
    };

    export struct EngineLifecycle {
        inline EngineLifecycle(const CStringView name, const int32 width, const int32 height) {
            Engine::initialize(name, width, height);
        }

        inline ~EngineLifecycle() {
            Engine::shutdown();
        }

        EngineLifecycle(const EngineLifecycle&) = delete;
        EngineLifecycle(EngineLifecycle&&) noexcept = delete;
        EngineLifecycle& operator=(const EngineLifecycle&) = delete;
        EngineLifecycle& operator=(EngineLifecycle&&) noexcept = delete;
    };
}