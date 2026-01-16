/**
 * @file geometry_render_object.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

module retro.runtime;

import retro.logging;

namespace retro
{
    struct SceneData
    {
        Vector2f viewport_size{};
        Vector2f transform_position{};
        float transform_rotation{};
        Vector2f transform_scale{};
    };

    RenderProxyID GeometryRenderObject::create_render_proxy(RenderProxyManager &proxy_manager)
    {
        return proxy_manager.emplace_proxy<GeometryRenderProxy>(*this);
    }

    void GeometryRenderObject::destroy_render_proxy(RenderProxyManager &proxy_manager, const RenderProxyID id)
    {
        proxy_manager.remove_proxy<GeometryRenderProxy>(id);
    }

    const Name GeometryRenderProxy::TYPE_ID = "geometry"_name;

    GeometryDrawCall GeometryRenderProxy::get_draw_call(const Vector2u viewport_size) const
    {
        const auto &object = *object_;
        GeometryDrawCall result{.geometry = object.geometry(),
                                .push_constants = std::vector<std::byte>(sizeof(GeometryRenderData))};

        *std::bit_cast<GeometryRenderData *>(result.push_constants.data()) = {
            .viewport_size = Vector2f{static_cast<float>(viewport_size.x), static_cast<float>(viewport_size.y)},
            .position = object.position(),
            .rotation = object.rotation(),
            .scale = object.scale()};

        return result;
    }

    usize GeometryRenderPipeline::push_constants_size() const
    {
        return sizeof(GeometryRenderData);
    }

    PipelineShaders GeometryRenderPipeline::shaders() const
    {

        return {"shaders/geometry.vert.spv", "shaders/geometry.frag.spv"};
    }

    void GeometryRenderPipeline::clear_draw_queue()
    {
        pending_geometry_.clear();
    }

    void GeometryRenderPipeline::queue_draw_calls(const std::any &render_data)
    {
        auto &data = std::any_cast<const std::vector<GeometryDrawCall> &>(render_data);
        pending_geometry_.append_range(data);
    }

    void GeometryRenderPipeline::execute(RenderContext &context)
    {
        context.draw_geometry(pending_geometry_);
    }
} // namespace retro
