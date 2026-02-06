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
import retro.core.async.manual_task_scheduler;
import retro.runtime.script_runtime;
import retro.runtime.rendering.renderer2d;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset_load_result;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.assets.asset;
import retro.runtime.world.scene;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.world.viewport;

namespace retro
{
    export class Engine
    {
      public:
        using Dependencies = TypeList<ScriptRuntime, Renderer2D, PipelineManager, AssetManager>;

        RETRO_API Engine(ScriptRuntime &script_runtime,
                         Renderer2D &renderer,
                         PipelineManager &pipeline_manager,
                         AssetManager &asset_manager);

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

        RETRO_API void run(std::u16string_view assembly_path,
                           std::u16string_view class_name,
                           std::u16string_view entry_point);

        RETRO_API void request_shutdown(std::int32_t exit_code = 0);

        [[nodiscard]] inline Scene &scene()
        {
            return scene_;
        }

        [[nodiscard]] inline ViewportManager &viewports()
        {
            return viewports_;
        }

        template <std::derived_from<Asset> T = Asset>
        AssetLoadResult<RefCountPtr<T>> load_asset(const AssetPath &path)
        {
            return asset_manager_->load_asset<T>(path);
        }

        RETRO_API bool remove_asset_from_cache(const AssetPath &path) const;

      private:
        void tick(float delta_time);
        void render();

        RETRO_API static Engine *instance_;

        friend struct AssetPathHook;

        ScriptRuntime *script_runtime_{};
        Renderer2D *renderer_{};
        AssetManager *asset_manager_{};

        std::atomic<std::int32_t> exit_code_{0};
        std::atomic<bool> running_{false};
        Scene scene_;
        ViewportManager viewports_;
        ManualTaskScheduler scheduler_{};
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
} // namespace retro
