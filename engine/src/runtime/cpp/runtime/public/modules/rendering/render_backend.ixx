/**
 * @file render_backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.render_backend;

import std;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;
import retro.runtime.rendering.texture;
import retro.runtime.rendering.image_data;
import retro.core.async.task;

namespace retro
{
    export class RenderBackend : public IntrusiveRefCounted
    {
      public:
        virtual ~RenderBackend() = default;

        virtual std::shared_ptr<Renderer2D> create_renderer(std::shared_ptr<Window> window) = 0;

        virtual Task<RefCountPtr<Texture>> upload_texture(std::span<const std::byte> bytes,
                                                          std::int32_t width,
                                                          std::int32_t height,
                                                          TextureFormat format,
                                                          TextureFilter filtering,
                                                          std::stop_token stop_token) = 0;

        inline Task<RefCountPtr<Texture>> upload_texture(const std::span<const std::byte> bytes,
                                                         const std::int32_t width,
                                                         const std::int32_t height,
                                                         const TextureFormat format,
                                                         const TextureFilter filtering)
        {
            return upload_texture(bytes, width, height, format, filtering, std::stop_token{});
        }

        inline Task<RefCountPtr<Texture>> upload_texture(const std::span<const std::byte> bytes,
                                                         const std::int32_t width,
                                                         const std::int32_t height,
                                                         const TextureFormat format,
                                                         std::stop_token stop_token = {})
        {
            return upload_texture(bytes, width, height, format, TextureFilter::nearest, std::move(stop_token));
        }

        inline Task<RefCountPtr<Texture>> upload_texture(const ImageData &image)
        {
            return upload_texture(image.bytes(), image.width(), image.height(), image.format());
        }
    };
} // namespace retro
