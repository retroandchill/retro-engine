/**
 * @file runtime.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.runtime;

export import :assets;
export import :scene;
export import :scene.rendering;
export import :scene.rendering.geometry_render_component;

import sdl;

namespace retro
{
    export class SdlRuntime
    {
      public:
        inline SdlRuntime()
        {
            if (!sdl::Init(sdl::InitFlags::VIDEO))
            {
                throw std::runtime_error{std::string{"SDL_Init failed: "} + sdl::GetError()};
            }
        }

        SdlRuntime(const SdlRuntime &) = delete;
        SdlRuntime(SdlRuntime &&) = delete;

        inline ~SdlRuntime() noexcept
        {
            sdl::Quit();
        }

        SdlRuntime &operator=(const SdlRuntime &) = delete;
        SdlRuntime &operator=(SdlRuntime &&) = delete;
    };

    export class ScriptRuntime
    {
      public:
        virtual ~ScriptRuntime() = default;

        [[nodiscard]] virtual int32 start_scripts(std::u16string_view assembly_path,
                                                  std::u16string_view class_name) const = 0;

        virtual void tick(float delta_time) = 0;

        virtual void tear_down() = 0;
    };

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

    export class Engine
    {
      public:
        RETRO_API explicit Engine(const EngineConfig &config);

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

        RETRO_API void run(std::u16string_view assembly_path,
                           std::u16string_view class_name,
                           std::u16string_view entry_point);

        RETRO_API void request_shutdown(int32 exit_code = 0);

        [[nodiscard]] inline Scene &scene() const
        {
            assert(scene_ != nullptr);
            return *scene_;
        }

      private:
        void tick(float delta_time) const;
        void render();

        RETRO_API static std::unique_ptr<Engine> instance_;

        std::unique_ptr<ScriptRuntime> script_runtime_{};
        std::unique_ptr<Renderer2D> renderer_{};

        std::atomic<int32> exit_code_{0};
        std::atomic<bool> running_{false};
        std::unique_ptr<Scene> scene_{};
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
