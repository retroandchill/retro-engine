//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:vulkan_renderer2d;

import retro.runtime;

namespace retro
{
    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        void begin_frame() override;

        void end_frame() override;

        void draw_quad(Vector2 position, Vector2 size, Color color) override;
    };
} // namespace retro