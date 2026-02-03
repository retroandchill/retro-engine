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
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_source;
import retro.runtime.assets.asset_decoder;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset_load_result;
import retro.runtime.assets.filesystem_asset_source;
import retro.runtime.assets.textures.texture_decoder;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.assets.asset;
import retro.runtime.scene;

namespace retro
{
    export class Engine
    {
      public:
        using Dependencies = TypeList<ScriptRuntime, Renderer2D, SceneDrawProxy, AssetManager>;

        inline Engine(ScriptRuntime &script_runtime,
                      Renderer2D &renderer,
                      SceneDrawProxy &draw_proxy,
                      AssetManager &asset_manager)
            : script_runtime_(&script_runtime), renderer_(&renderer), asset_manager_{&asset_manager}, scene_{draw_proxy}
        {
        }

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

        inline static void initialize(std::unique_ptr<Engine> engine)
        {
            assert(instance_ == nullptr);
            instance_ = std::move(engine);
        }

        inline static void shutdown()
        {
            instance_.reset();
        }

        RETRO_API void run(std::u16string_view assembly_path,
                           std::u16string_view class_name,
                           std::u16string_view entry_point);

        RETRO_API void request_shutdown(std::int32_t exit_code = 0);

        [[nodiscard]] inline Scene &scene()
        {
            return scene_;
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

        RETRO_API static std::unique_ptr<Engine> instance_;

        friend struct AssetPathHook;

        ScriptRuntime *script_runtime_{};
        Renderer2D *renderer_{};
        AssetManager *asset_manager_{};

        std::atomic<std::int32_t> exit_code_{0};
        std::atomic<bool> running_{false};
        Scene scene_;
        ManualTaskScheduler scheduler_{};
    };

    export struct EngineLifecycle
    {
        explicit inline EngineLifecycle(std::unique_ptr<Engine> engine)
        {
            Engine::initialize(std::move(engine));
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

    export inline auto add_engine_services(ServiceCollection &services)
    {
        services.add_transient<Engine>()
            .add_singleton<SceneDrawProxy, PipelineManager>()
            .add_singleton<RenderPipeline, GeometryRenderPipeline>()
            .add_singleton<RenderPipeline, SpriteRenderPipeline>()
            .add_singleton<AssetSource, FileSystemAssetSource>()
            .add_singleton<AssetManager>()
            .add_singleton<AssetDecoder, TextureDecoder>();
    }
} // namespace retro
