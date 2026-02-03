/**
 * @file texture.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.assets.textures.texture;

import retro.runtime.assets.asset;
import retro.runtime.assets.asset_path;
import retro.runtime.rendering.texture_render_data;
import retro.core.strings.name;
import std;

namespace retro
{

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
