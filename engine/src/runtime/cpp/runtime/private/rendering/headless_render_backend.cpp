/**
 * @file headless_render_backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.headless_render_backend;
import retro.runtime.rendering.headless_renderer2d;

namespace retro
{
    class HeadlessTextureRenderData final : public TextureRenderData
    {
      public:
        HeadlessTextureRenderData(const std::int32_t width, const std::int32_t height)
            : TextureRenderData(width, height)
        {
        }
    };

    std::shared_ptr<Renderer2D> HeadlessRenderBackend::create_renderer(std::shared_ptr<Window> window)
    {
        return std::make_shared<HeadlessRenderer2D>(std::move(window));
    }
    std::unique_ptr<TextureRenderData> HeadlessRenderBackend::upload_texture(std::span<const std::byte> bytes,
                                                                             std::int32_t width,
                                                                             std::int32_t height)
    {
        return std::make_unique<HeadlessTextureRenderData>(width, height);
    }
} // namespace retro
