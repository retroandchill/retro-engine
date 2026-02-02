/**
 * @file texture.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:texture;

import :assets;

namespace retro
{
    struct ImageDataDeleter
    {
        RETRO_API void operator()(std::byte *bytes) const;
    };

    using ImageDataPtr = std::unique_ptr<std::byte[], ImageDataDeleter>;

    export struct ImageData
    {
        ImageDataPtr image_data;
        std::int32_t width;
        std::int32_t height;
        std::int32_t channels;
    };

    export class TextureRenderData
    {
      protected:
        inline TextureRenderData(std::int32_t width, std::int32_t height) noexcept : width_(width), height_(height)
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

    export class RETRO_API TextureDecoder final : public AssetDecoder
    {
      public:
        using Dependencies = TypeList<class Renderer2D>;

        explicit inline TextureDecoder(Renderer2D &renderer) : renderer_{&renderer}
        {
        }

        [[nodiscard]] bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const override;

        AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context, BufferedStream &stream) override;

      private:
        static AssetLoadResult<ImageData> load_image_data(std::span<const std::byte> bytes) noexcept;

        Renderer2D *renderer_;
    };

    export class RETRO_API Texture final : public Asset
    {
      public:
        explicit Texture(const AssetPath &path, std::unique_ptr<TextureRenderData> render_data)
            : Asset(path), render_data_{std::move(render_data)}
        {
        }

        [[nodiscard]] Name asset_type() const noexcept override;

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

        void on_engine_shutdown() override;

      private:
        friend class TextureDecoder;

        std::unique_ptr<TextureRenderData> render_data_;
    };
} // namespace retro
