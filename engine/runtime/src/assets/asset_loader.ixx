/**
 * @file asset_loader.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:assets.asset_loader;

import retro.core;
import :assets.asset;

namespace retro
{
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
} // namespace retro
