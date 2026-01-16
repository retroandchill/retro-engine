/**
 * @file registrations.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import retro.runtime;
import retro.renderer;
import vulkan_hpp;
import std;

namespace retro
{
    const RenderObjectTypeRegistration quad_registration{
        "Quad",
        [](const ViewportID viewport_id) -> QuadRenderComponent &
        {
            return Engine::instance().scene().create_render_object<QuadRenderComponent>(viewport_id);
        }};

    const PipelineRegistration quad_pipeline_registration{
        "Quad",
        [](vk::Device device,
           const VulkanSwapchain &swapchain,
           vk::RenderPass render_pass) -> std::unique_ptr<RenderPipeline>
        {
            return std::make_unique<QuadRenderPipeline>(device, swapchain, render_pass);
        }};
} // namespace retro
