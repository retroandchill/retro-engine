/**
 * @file texture.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.texture;

import std;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export class TextureRenderData
    {
      protected:
        inline TextureRenderData(const std::int32_t width, const std::int32_t height) noexcept
            : width_(width), height_(height)
        {
        }

      public:
        virtual ~TextureRenderData() = default;

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return width_;
        }
        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return height_;
        }

      private:
        std::int32_t width_{};
        std::int32_t height_{};
    };

    export class Texture final : public IntrusiveRefCounted
    {
      public:
        [[nodiscard]] inline const TextureRenderData *render_data() const noexcept
        {
            return render_data_.get();
        }

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return render_data_->width();
        }

        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return render_data_->height();
        }

        inline void clear_render_data() noexcept
        {
            render_data_.reset();
        }

      private:
        std::unique_ptr<TextureRenderData> render_data_;
    };
} // namespace retro
