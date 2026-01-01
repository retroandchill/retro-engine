//
// Created by fcors on 12/31/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:instances.quad;

import std;
import vulkan_hpp;
import :components;
import :pipeline;
import retro.runtime;

namespace retro
{
    export struct Quad
    {
        Vector2f position{};
        Vector2f size{};
        Color color{};
    };

    export class QuadRenderComponent final : public RenderComponent
    {
      public:
        inline explicit QuadRenderComponent(Entity &entity) : RenderComponent{entity}
        {
        }

        void create_render_proxy(RenderProxyManager &proxy_manager) override;

        void destroy_render_proxy(RenderProxyManager &proxy_manager) override;

        inline const Color &color() const noexcept
        {
            return color_;
        }

        inline void set_color(const Color color) noexcept
        {
            color_ = color;
        }

        inline Vector2f size() const noexcept
        {
            auto &[scale_x, scale_y] = entity().transform().scale;
            return {size_.x * scale_x, size_.y * scale_y};
        }

        inline void set_size(const Vector2f size) noexcept
        {
            size_ = size;
        }

      private:
        Vector2f size_;
        Color color_{};
    };

    export class QuadRenderProxy
    {
      public:
        using DrawCallData = Quad;

        inline QuadRenderProxy(const uint64 id, QuadRenderComponent &component) : id_(id), component_(&component)
        {
        }

        inline static Name type_id()
        {
            return TYPE_ID;
        }

        inline uint64 id() const
        {
            return id_;
        }

        Quad get_draw_call() const;

      private:
        static const Name TYPE_ID;

        uint64 id_;
        QuadRenderComponent *component_;
    };

    export class RETRO_API QuadRenderPipeline final : public RenderPipeline
    {
      public:
        inline QuadRenderPipeline(const vk::Device device,
                                  const VulkanSwapchain &swapchain,
                                  const vk::RenderPass render_pass)
            : pipeline_layout_{create_pipeline_layout(device)},
              graphics_pipeline_{create_graphics_pipeline(device, pipeline_layout_.get(), swapchain, render_pass)}
        {
        }

        inline Name type() const override
        {
            return QuadRenderProxy::type_id();
        }

        void recreate(vk::Device device, const VulkanSwapchain &swapchain, vk::RenderPass render_pass) override;
        void queue_draw_calls(const std::any &render_data) override;
        void draw_quad(Vector2f position, Vector2f size, Color color);

        void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size) override;
        void clear_draw_queue() override;

      private:
        static vk::UniquePipelineLayout create_pipeline_layout(vk::Device device);
        static vk::UniquePipeline create_graphics_pipeline(vk::Device device,
                                                           vk::PipelineLayout layout,
                                                           const VulkanSwapchain &swapchain,
                                                           vk::RenderPass render_pass);
        static vk::UniqueShaderModule create_shader_module(vk::Device device, const std::filesystem::path &path);

      private:
        vk::UniquePipelineLayout pipeline_layout_;
        vk::UniquePipeline graphics_pipeline_;
        std::vector<Quad> pending_quads_;
    };
} // namespace retro