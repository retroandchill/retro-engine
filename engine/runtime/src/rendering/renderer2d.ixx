/**
 * @file rendering.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

export module retro.runtime.rendering.renderer2d;

import retro.core.containers.inline_list;
import retro.core.containers.optional;
import retro.core.math.vector;
import retro.logging;
import std;
import retro.core.di;
import retro.runtime.rendering.texture_render_data;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.world.viewport;

namespace retro
{

    export class Renderer2D
    {
      public:
        virtual ~Renderer2D() = default;

        virtual void wait_idle() = 0;

        virtual void begin_frame() = 0;

        virtual void end_frame() = 0;

        virtual void add_new_render_pipeline(std::type_index type, RenderPipeline &pipeline) = 0;

        virtual void remove_render_pipeline(std::type_index type) = 0;

        virtual std::unique_ptr<TextureRenderData> upload_texture(const ImageData &image_data) = 0;

        [[nodiscard]] virtual Vector2u window_size() const = 0;

        virtual void add_viewport(Viewport &viewport) = 0;

        virtual void remove_viewport(Viewport &viewport) = 0;
    };

} // namespace retro
