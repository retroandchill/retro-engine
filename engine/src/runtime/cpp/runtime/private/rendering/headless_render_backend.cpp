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
    class HeadlessTexture final : public Texture
    {
      public:
        HeadlessTexture(std::vector<std::byte> data,
                        const std::int32_t width,
                        const std::int32_t height,
                        const TextureFormat format)
            : Texture{width, height, format}, data_(std::move(data))
        {
        }

        [[nodiscard]] std::span<const std::byte> data() const
        {
            return std::span(data_);
        }

      private:
        std::vector<std::byte> data_;
    };

    std::shared_ptr<Renderer2D> HeadlessRenderBackend::create_renderer(std::shared_ptr<Window> window)
    {
        return std::make_shared<HeadlessRenderer2D>(std::move(window));
    }
    RefCountPtr<Texture> HeadlessRenderBackend::upload_texture(std::span<const std::byte> bytes,
                                                               std::int32_t width,
                                                               std::int32_t height,
                                                               TextureFormat format)
    {
        return make_ref_counted<HeadlessTexture>(bytes | std::ranges::to<std::vector>(), width, height, format);
    }
    std::pair<bool, std::size_t> HeadlessRenderBackend::export_texture(const Texture &texture,
                                                                       std::span<std::byte> buffer)
    {
        const auto &data = dynamic_cast<const HeadlessTexture &>(texture).data();
        if (buffer.size() < data.size())
        {
            return {false, 0};
        }

        std::ranges::copy(data, buffer.begin());
        return {true, data.size()};
    }
} // namespace retro
