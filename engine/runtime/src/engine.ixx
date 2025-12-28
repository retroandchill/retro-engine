//
// Created by fcors on 12/21/2025.
//
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.runtime:engine;

import std;
import retro.core;
import :interfaces;

namespace retro
{
    export template <typename T>
    struct EngineDependencyFactory
    {
        template <std::invocable<> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor>, std::unique_ptr<T>>
        explicit(false) EngineDependencyFactory(Functor &&factory)
            : factory_([factory = std::forward<Functor>(factory)] -> std::unique_ptr<T> { return factory(); })
        {
        }

        std::unique_ptr<T> operator()() const
        {
            return factory_();
        }

      private:
        std::function<std::unique_ptr<T>()> factory_{};
    };

    export struct EngineConfig
    {
        EngineDependencyFactory<ScriptRuntime> script_runtime_factory;
        EngineDependencyFactory<Renderer2D> renderer_factory;
    };

    export class RETRO_API Engine
    {
      public:
        explicit Engine(const EngineConfig &config);

        ~Engine() = default;

        Engine(const Engine &) = delete;
        Engine(Engine &&) noexcept = delete;
        Engine &operator=(const Engine &) = delete;
        Engine &operator=(Engine &&) noexcept = delete;

        static inline Engine &instance()
        {
            assert(instance_ != nullptr);
            return *instance_;
        }

        inline static void initialize(const EngineConfig &config)
        {
            assert(instance_ == nullptr);
            instance_ = std::make_unique<Engine>(config);
        }

        inline static void shutdown()
        {
            instance_.reset();
        }

        void run();

        void request_shutdown();

      private:
        void tick(float delta_time);
        void render();

        static std::unique_ptr<Engine> instance_;

        std::unique_ptr<ScriptRuntime> script_runtime{};
        std::unique_ptr<Renderer2D> renderer_{};

        std::atomic<bool> running_{false};
    };

    export struct EngineLifecycle
    {
        explicit inline EngineLifecycle(const EngineConfig &config)
        {
            Engine::initialize(config);
        }

        inline ~EngineLifecycle()
        {
            Engine::shutdown();
        }

        EngineLifecycle(const EngineLifecycle &) = delete;
        EngineLifecycle(EngineLifecycle &&) noexcept = delete;
        EngineLifecycle &operator=(const EngineLifecycle &) = delete;
        EngineLifecycle &operator=(EngineLifecycle &&) noexcept = delete;
    };
} // namespace retro