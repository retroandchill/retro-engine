/**
 * @file headless_render_target.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.headless_render_target;

import std;
import retro.core.math.vector;
import retro.runtime.rendering.render_target;
import retro.runtime.rendering.texture;
import retro.platform.window;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export class HeadlessWindowRenderTarget final : public WindowRenderTarget
    {
      public:
        explicit inline HeadlessWindowRenderTarget(const std::uint64_t id, std::unique_ptr<Window> window)
            : id_{id}, window_{std::move(window)}
        {
        }

        [[nodiscard]] inline std::uint64_t id() const noexcept override
        {
            return id_;
        }

        [[nodiscard]] inline Vector2u size() const noexcept override
        {
            return window_->size();
        }

        [[nodiscard]] inline TextureFormat format() const noexcept override
        {
            return TextureFormat::rgba8;
        }

        [[nodiscard]] inline Window &window() const noexcept override
        {
            return *window_;
        }

      private:
        std::uint64_t id_;
        std::unique_ptr<Window> window_;
    };

    export class HeadlessTextureRenderTarget final : public TextureRenderTarget
    {
      public:
        explicit inline HeadlessTextureRenderTarget(std::uint64_t id, RefCountPtr<Texture> texture)
            : id_{id}, texture_{std::move(texture)}
        {
        }

        [[nodiscard]] inline std::uint64_t id() const noexcept override
        {
            return id_;
        }

        [[nodiscard]] inline Vector2u size() const noexcept override
        {
            return Vector2u{static_cast<std::uint32_t>(texture_->width()),
                            static_cast<std::uint32_t>(texture_->height())};
        }

        [[nodiscard]] inline TextureFormat format() const noexcept override
        {
            return texture_->format();
        }

        [[nodiscard]] inline const RefCountPtr<Texture> &texture() const noexcept override
        {
            return texture_;
        }

      private:
        std::uint64_t id_;
        RefCountPtr<Texture> texture_;
    };
} // namespace retro
