//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:vulkan_renderer2d;

import retro.runtime;
import :window;
import :components;

namespace retro
{
    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        explicit VulkanRenderer2D(Window window);

        void begin_frame() override;

        void end_frame() override;

        void draw_quad(Vector2 position, Vector2 size, Color color) override;

    private:
        static VulkanInstance get_instance_create_info();
        static std::span<const char * const> get_required_instance_extensions();

        Window window_;

        VulkanInstance instance_;
        VulkanSurface surface_;
        VulkanDevice device_;
    };
} // namespace retro