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
    export enum class TextureFilter : std::uint8_t
    {
        nearest = 0,
        linear = 1
    };

    export enum class TextureFormat : std::uint8_t
    {
        rgba8,
        unorm,
        rgba16f
    };

    export class Texture : public IntrusiveRefCounted
    {
      protected:
        inline Texture(const std::int32_t width,
                       const std::int32_t height,
                       const TextureFormat format,
                       const TextureFilter filter) noexcept
            : width_{width}, height_{height}, format_{format}, filter_{filter}
        {
        }

      public:
        virtual ~Texture() = default;

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return width_;
        }
        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return height_;
        }

        [[nodiscard]] inline TextureFormat format() const noexcept
        {
            return format_;
        }

        [[nodiscard]] inline TextureFilter filter() const noexcept
        {
            return filter_;
        }

      private:
        std::int32_t width_{};
        std::int32_t height_{};
        TextureFormat format_{};
        TextureFilter filter_{TextureFilter::nearest};
    };
} // namespace retro
