/**
 * @file engine.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <cassert>

export module retro.runtime.engine;

import std;
import retro.core.di;
import retro.core.async.task;
import retro.core.async.manual_task_scheduler;
import retro.core.functional.delegate;
import retro.runtime.rendering.renderer2d;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset_load_result;
import retro.core.memory.ref_counted_ptr;
import retro.platform.window;
import retro.core.containers.optional;
import retro.runtime.assets.asset;
import retro.runtime.world.scene;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.world.viewport;
import retro.platform.backend;
import retro.platform.event;
import retro.core.functional.function_ref;
import retro.core.type_traits.range;

namespace retro
{
    export using OnWindowRemoved = MulticastDelegate<void(const Window &)>;
    export using OnShutdownRequested = MulticastDelegate<void()>;

    export class Engine
    {
      public:
        RETRO_API explicit Engine(std::shared_ptr<ServiceProvider> service_provider);

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

        inline static void initialize(Engine &engine)
        {
            assert(instance_ == nullptr);
            instance_ = std::addressof(engine);
        }

        inline static void shutdown()
        {
            instance_ = nullptr;
        }

        RETRO_API void pump_tasks(std::size_t max = index_none<std::size_t>);

        RETRO_API void render();

        RETRO_API void on_loop_exit();

        RETRO_API void wait_platform_event(std::chrono::milliseconds timeout);

        RETRO_API void poll_events_once();

        inline OnShutdownRequested::Event on_shutdown_requested()
        {
            return on_shutdown_requested_;
        }

        [[nodiscard]] inline Optional<Renderer2D &> primary_renderer() const noexcept
        {
            return primary_renderer_;
        }

        RETRO_API Task<PlatformResult<std::shared_ptr<Window>>> create_new_window(WindowDesc window_desc);

        RETRO_API void add_window(Window &window);

        RETRO_API void remove_window(const Window &window);

        [[nodiscard]] inline SceneManager &scenes()
        {
            return scenes_;
        }

        [[nodiscard]] inline ViewportManager &viewports()
        {
            return viewports_;
        }

        template <std::derived_from<Asset> T = Asset>
        Optional<T &> load_asset_from_cache(const AssetPath &path)
        {
            return asset_manager_.load_from_cache<T>(path);
        }

        template <std::derived_from<Asset> T = Asset>
        Optional<RefCountPtr<T>> load_asset(const AssetPath &path, std::span<const std::byte> buffer)
        {
            return asset_manager_.load_asset<T>(path, buffer);
        }

        RETRO_API bool remove_asset_from_cache(const AssetPath &path) const;

        inline OnWindowRemoved::Event on_window_removed()
        {
            return on_window_removed_;
        }

      private:
        bool handle_platform_event(const Event &event);

        RETRO_API static Engine *instance_;

        friend struct AssetPathHook;

        std::shared_ptr<ServiceProvider> service_provider_{};
        PlatformBackend &platform_backend_;
        ServiceScopeFactory &service_scope_factory_;
        std::shared_mutex renderers_mutex_;
        std::map<std::uint64_t, RendererRef> renderers_;
        Optional<Renderer2D &> primary_renderer_;
        AssetManager &asset_manager_;
        PipelineManager &pipeline_manager_;

        SceneManager scenes_;
        ViewportManager viewports_;
        ManualTaskScheduler scheduler_{};

        OnWindowRemoved on_window_removed_;
        OnShutdownRequested on_shutdown_requested_;
    };

    export struct EngineLifecycle
    {
        explicit inline EngineLifecycle(Engine &engine)
        {
            Engine::initialize(engine);
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

    export RETRO_API void add_engine_services(ServiceCollection &services);

    export struct EngineConfigContext
    {
        ServiceCollection services;
    };
} // namespace retro
