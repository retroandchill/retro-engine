/**
 * @file asset.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.assets.asset;

import retro.runtime.engine;

namespace retro
{
    void AssetPathHook::reset() noexcept
    {
        if (path.is_valid())
            (void)Engine::instance().remove_asset_from_cache(path);

        path = AssetPath::none();
    }

    void AssetPathHook::release() noexcept
    {
        path = AssetPath::none();
    }

    void Asset::on_engine_shutdown()
    {
        hook_.release();
    }
} // namespace retro
