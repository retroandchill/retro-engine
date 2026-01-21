/**
 * @file texture.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:assets.textures.texture;

import retro.core;
import :assets.asset;

namespace retro
{
    export class Texture : public Asset
    {
      public:
        explicit Texture(const AssetPath &path) : Asset{path}
        {
        }
    };
} // namespace retro
