/**
 * @file asset_path.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:assets.asset_path;

import retro.core;
import std;

namespace retro
{
    export struct AssetPath
    {
        static constexpr auto PACKAGE_SEPARATOR = ':';

        constexpr AssetPath() = default;

        constexpr AssetPath(const Name package_name, const Name asset_name) noexcept
            : package_name_{package_name}, asset_name_{asset_name}
        {
        }

        template <std::ranges::input_range Range>
            requires Char<std::ranges::range_value_t<Range>>
        explicit AssetPath(Range &&range)
        {
            usize segments = 0;
            for (auto &&inner_view : range | std::views::lazy_split(PACKAGE_SEPARATOR))
            {
                segments++;

                if (segments == 1)
                {
                    package_name_ = Name{inner_view};
                }
                else if (segments == 2)
                {
                    asset_name_ = Name{inner_view};
                }
                else
                {
                    break;
                }
            }

            if (segments < 2)
            {
                throw std::invalid_argument{"Invalid asset path"};
            }
        }

        inline Name package_name() const noexcept
        {
            return package_name_;
        }

        inline Name asset_name() const noexcept
        {
            return asset_name_;
        }

        template <Char CharType = char, SimpleAllocator Allocator = std::allocator<CharType>>
            requires std::same_as<CharType, typename Allocator::value_type>
        [[nodiscard]] auto to_string(Allocator allocator = Allocator{}) const
        {
            auto target_string = package_name_.to_string<CharType>(allocator);
            target_string.push_back(PACKAGE_SEPARATOR);
            asset_name_.append_string(target_string);
            return target_string;
        }

        template <Char CharType, SimpleAllocator Allocator>
            requires std::same_as<CharType, typename Allocator::value_type>
        void append_string(std::basic_string<CharType, std::char_traits<CharType>, Allocator> &target_string) const
        {
            package_name_.append_string(target_string);
            target_string.push_back(PACKAGE_SEPARATOR);
            asset_name_.append_string(target_string);
        }

      private:
        Name package_name_{};
        Name asset_name_{};
    };
} // namespace retro
