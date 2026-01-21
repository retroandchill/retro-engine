/**
 * @file asset.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:assets.asset;

import retro.core;
import boost;
import :assets.asset_path;

namespace retro
{
    export class Asset : public IntrusiveRefCounted
    {
      protected:
        explicit Asset(const AssetPath &path) : path_{path}
        {
        }

      public:
        [[nodiscard]] inline AssetPath path() const noexcept
        {
            return path_;
        }

      private:
        using Hook = boost::intrusive::set_member_hook<boost::intrusive::link_mode<boost::intrusive::auto_unlink>>;

        AssetPath path_;
        Hook hook_;

        struct AssetPathKey
        {
            using type = AssetPath;

            inline const AssetPath &operator()(const Asset &asset) const noexcept
            {
                return asset.path_;
            }
        };

        friend class boost::intrusive::set<Asset,
                                           boost::intrusive::member_hook<Asset, Hook, &Asset::hook_>,
                                           boost::intrusive::constant_time_size<false>,
                                           boost::intrusive::key_of_value<AssetPathKey>>;

      public:
        using Map = boost::intrusive::set<Asset,
                                          boost::intrusive::member_hook<Asset, Hook, &Asset::hook_>,
                                          boost::intrusive::constant_time_size<false>,
                                          boost::intrusive::key_of_value<AssetPathKey>>;
    };
} // namespace retro
