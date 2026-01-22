/**
 * @file assets.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:assets;

import retro.core;
import boost;
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

        [[nodiscard]] inline bool is_valid() const noexcept
        {
            return package_name_.is_valid() && !package_name_.is_none() && asset_name_.is_valid() &&
                   !asset_name_.is_none();
        }

        static constexpr AssetPath none() noexcept
        {
            return AssetPath{};
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

        friend constexpr bool operator==(const AssetPath &lhs, const AssetPath &rhs) noexcept = default;

        friend constexpr std::strong_ordering operator<=>(const AssetPath &lhs,
                                                          const AssetPath &rhs) noexcept = default;

        friend constexpr usize hash_value(const AssetPath path)
        {
            return hash_combine(path.package_name_, path.asset_name_);
        }

      private:
        Name package_name_{};
        Name asset_name_{};
    };
} // namespace retro

template <>
struct std::hash<retro::AssetPath>
{
    constexpr usize operator()(const retro::AssetPath &path) const noexcept
    {
        return hash_value(path);
    }
};

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

    export class BadAssetPathError
    {
        // TODO: Fill out
    };

    export class InvalidAssetFormatError
    {
        // TODO: Fill out
    };

    export class AmbiguousAssetPathError
    {
        // TODO: Fill out
    };

    export class AssetNotFound
    {
        // TODO: Fill out
    };

    export class AssetTypeMismatch
    {
      public:
        AssetTypeMismatch(const std::type_info &expected, const std::type_info &actual)
            : expected_type_(std::addressof(expected)), actual_type_(std::addressof(actual))
        {
        }

      private:
        const std::type_info *expected_type_{};
        const std::type_info *actual_type_{};
    };

    export class AssetLoadError
    {
        using Storage = std::variant<BadAssetPathError,
                                     InvalidAssetFormatError,
                                     AmbiguousAssetPathError,
                                     AssetNotFound,
                                     AssetTypeMismatch>;

      public:
        template <typename T, typename... Args>
            requires VariantMember<T, Storage> && std::constructible_from<T, Args...>
        explicit constexpr AssetLoadError(std::in_place_type_t<T>, Args &&...args)
            : error_{std::in_place_type<T>, std::forward<Args>(args)...}
        {
        }

        template <CanVisitVariant<Storage> Functor>
        constexpr decltype(auto) visit(Functor &&functor) const
        {
            return std::visit(std::forward<Functor>(functor), error_);
        }

      private:
        Storage error_;
    };

    export class AssetLoader
    {
      public:
        virtual ~AssetLoader() = default;

        virtual std::expected<RefCountPtr<Asset>, AssetLoadError> load_asset_from_path(AssetPath path) = 0;
    };

    export class RETRO_API AssetManager
    {
      public:
        explicit AssetManager(std::unique_ptr<AssetLoader> asset_loader) : asset_loader_{std::move(asset_loader)}
        {
        }

        template <std::derived_from<Asset> T>
        std::expected<RefCountPtr<T>, AssetLoadError> load_asset(const AssetPath &path)
        {
            if constexpr (std::is_same_v<T, Asset>)
            {
                return load_asset_internal(path);
            }
            else
            {
                return load_asset_internal(path).and_then(
                    [](RefCountPtr<Asset> &&asset) -> std::expected<RefCountPtr<T>, AssetLoadError>
                    {
                        auto *asset_ptr = asset.get();
                        auto cast_ptr = dynamic_pointer_cast<T>(std::move(asset));
                        if (cast_ptr == nullptr)
                        {
                            return std::unexpected{
                                AssetLoadError{std::in_place_type<AssetTypeMismatch>, typeid(T), typeid(*asset_ptr)}};
                        }

                        return std::move(cast_ptr);
                    });
            }
        }

      private:
        std::expected<RefCountPtr<Asset>, AssetLoadError> load_asset_internal(const AssetPath &path);

        std::unique_ptr<AssetLoader> asset_loader_{};
        Asset::Map asset_cache_{};
    };
} // namespace retro
