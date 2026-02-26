/**
 * @file engine_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.engine;
import retro.core.di;
import retro.platform.backend;
import retro.platform.window;
import retro.runtime.rendering.pipeline_manager;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.runtime.assets.asset_source;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_decoder;
import retro.runtime.assets.filesystem_asset_source;
import retro.runtime.assets.textures.texture_decoder;

namespace
{
    using ConfigCallback = void (*)(retro::EngineConfigContext *, void *);
}

extern "C"
{
    RETRO_API retro::Engine *retro_create_engine(const retro::PlatformBackendInfo platform_config,
                                                 const ConfigCallback config_callback,
                                                 void *user_data)
    {
        retro::EngineConfigContext context;
        context.services.add(retro::PlatformBackend::create(platform_config));
        context.services.add_singleton<retro::PipelineManager>()
            .add_singleton<retro::RenderPipeline, retro::GeometryRenderPipeline>()
            .add_singleton<retro::RenderPipeline, retro::SpriteRenderPipeline>()
            .add_singleton<retro::AssetSource, retro::FileSystemAssetSource>()
            .add_singleton<retro::AssetManager>()
            .add_singleton<retro::AssetDecoder, retro::TextureDecoder>();

        if (config_callback != nullptr)
        {
            config_callback(std::addressof(context), user_data);
        }

        auto engine = std::make_unique<retro::Engine>(context.services.create_service_provider());
        retro::Engine::initialize(*engine);
        return engine.release();
    }

    RETRO_API std::uint64_t retro_engine_create_main_window(retro::Engine *engine,
                                                            const char *window_title,
                                                            std::int32_t width,
                                                            std::int32_t height,
                                                            retro::WindowFlags flags)
    {
        auto &window = engine->create_new_window(
            retro::WindowDesc{.width = width, .height = height, .title = window_title, .flags = flags});
        return window.id();
    }

    RETRO_API void retro_destroy_engine(retro::Engine *engine)
    {
        // Put the engine back into a unique_ptr so no matter what happens here it always get destroyed
        std::unique_ptr<retro::Engine> ptr(engine);
        retro::Engine::shutdown();
    }

    RETRO_API void retro_engine_run(retro::Engine *engine)
    {
        engine->run();
    }

    RETRO_API void retro_engine_poll_platform_events(retro::Engine *engine)
    {
        engine->run_platform_event_loop();
    }

    RETRO_API void retro_engine_request_shutdown(const std::int32_t exit_code)
    {
        retro::Engine::instance().request_shutdown(exit_code);
    }
}
