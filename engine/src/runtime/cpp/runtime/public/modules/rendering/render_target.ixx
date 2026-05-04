/**
 * @file render_target.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.render_target;

import std;
import retro.core.math.vector;
import retro.runtime.rendering.texture;
import retro.platform.window;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export enum class RenderTargetKind : std::uint8_t
    {
        window,
        texture
    };

    export class RenderTarget
    {
      protected:
        explicit RenderTarget(const std::uint64_t id) noexcept : id_{id}
        {
        }

      public:
        virtual ~RenderTarget() = default;

        [[nodiscard]] inline std::uint64_t id() const noexcept
        {
            return id_;
        }

        [[nodiscard]] virtual RenderTargetKind kind() const noexcept = 0;

        [[nodiscard]] virtual Vector2u size() const noexcept = 0;

        [[nodiscard]] virtual TextureFormat format() const noexcept = 0;

      private:
        std::uint64_t id_{};
    };

    export class WindowRenderTarget : public RenderTarget
    {
      protected:
        explicit WindowRenderTarget(const std::uint64_t id) noexcept : RenderTarget{id}
        {
        }

      public:
        [[nodiscard]] RenderTargetKind kind() const noexcept final
        {
            return RenderTargetKind::window;
        }

        [[nodiscard]] virtual Window &window() const noexcept = 0;
    };

    export class TextureRenderTarget : public RenderTarget
    {
      protected:
        explicit TextureRenderTarget(const std::uint64_t id) noexcept : RenderTarget{id}
        {
        }

      public:
        [[nodiscard]] RenderTargetKind kind() const noexcept final
        {
            return RenderTargetKind::texture;
        }

        [[nodiscard]] virtual const RefCountPtr<Texture> &texture() const noexcept = 0;
    };
} // namespace retro
