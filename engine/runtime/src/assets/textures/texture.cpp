/**
 * @file texture.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.assets.textures.texture;

namespace retro
{
    Name Texture::asset_type() const noexcept
    {
        static Name type{"Texture"};
        return type;
    }

    void Texture::on_engine_shutdown()
    {
        Asset::on_engine_shutdown();
        render_data_.reset();
    }

} // namespace retro
