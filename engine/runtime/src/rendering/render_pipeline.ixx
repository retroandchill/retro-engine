/**
 * @file render_pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.render_pipeline;

import std;
import retro.runtime.rendering.shader_layout;
import retro.runtime.rendering.draw_command;
import retro.runtime.world.scene_node;
import retro.runtime.world.viewport;
import retro.core.math.vector;
import retro.core.di;

namespace retro
{
    export class RenderContext
    {
      public:
        virtual ~RenderContext() = default;

        virtual void draw(std::span<const DrawCommand> command, const ShaderLayout &layout) = 0;
    };

    export class RenderPipeline
    {
      public:
        virtual ~RenderPipeline() = default;

        [[nodiscard]] virtual std::type_index component_type() const = 0;

        [[nodiscard]] virtual const ShaderLayout &shaders() const = 0;

        virtual void clear_draw_queue() = 0;

        virtual void collect_draw_calls(const SceneNodeList &nodes,
                                        Vector2u viewport_size,
                                        const Viewport &viewport) = 0;

        virtual void execute(RenderContext &context, const Viewport &viewport) = 0;
    };

} // namespace retro
