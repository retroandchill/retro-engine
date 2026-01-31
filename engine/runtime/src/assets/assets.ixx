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

        [[nodiscard]] inline Name package_name() const noexcept
        {
            return package_name_;
        }

        [[nodiscard]] inline Name asset_name() const noexcept
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
        return hash_combine(path.package_name(), path.asset_name());
    }
};

namespace retro
{
    struct AssetPathHook
    {
        AssetPath path;

        explicit inline AssetPathHook(const AssetPath &path) : path{path}
        {
        }

        AssetPathHook(const AssetPathHook &other) = delete;
        inline AssetPathHook(AssetPathHook &&other) noexcept : path{std::exchange(other.path, AssetPath::none())}
        {
        }

        inline ~AssetPathHook() noexcept
        {
            reset();
        }

        AssetPathHook &operator=(const AssetPathHook &other) = delete;
        inline AssetPathHook &operator=(AssetPathHook &&other) noexcept
        {
            reset();
            path = std::exchange(other.path, AssetPath::none());
            return *this;
        }

        RETRO_API void reset() noexcept;
    };

    export class Asset : public IntrusiveRefCounted
    {
      protected:
        explicit Asset(const AssetPath &path) : hook_{path}
        {
        }

      public:
        [[nodiscard]] inline AssetPath path() const noexcept
        {
            return hook_.path;
        }

        [[nodiscard]] virtual Name asset_type() const noexcept = 0;

      private:
        AssetPathHook hook_;
    };

    export enum class AssetLoadError : uint8
    {
        BadAssetPath,
        InvalidAssetFormat,
        AmbiguousAssetPath,
        AssetNotFound,
        AssetTypeMismatch
    };

    export template <typename T>
    using AssetLoadResult = std::expected<T, AssetLoadError>;

    export struct AssetOpenOptions
    {
        // TODO: Either remove or add options
    };

    export class AssetSource
    {
      public:
        virtual ~AssetSource() = default;

        inline AssetLoadResult<std::unique_ptr<Stream>> open_stream(const AssetPath &path)
        {
            return open_stream(path, {});
        }

        virtual AssetLoadResult<std::unique_ptr<Stream>> open_stream(AssetPath path,
                                                                     const AssetOpenOptions &options) = 0;
    };

    export class RETRO_API FileSystemAssetSource final : public AssetSource
    {
      public:
        AssetLoadResult<std::unique_ptr<Stream>> open_stream(AssetPath path, const AssetOpenOptions &options) override;
    };

    export struct AssetDecodeContext
    {
        AssetPath path{};
    };

    export class AssetDecoder
    {
      public:
        virtual ~AssetDecoder() = default;

        [[nodiscard]] virtual bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const = 0;

        virtual AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context,
                                                           BufferedStream &stream) = 0;
    };

    export class RETRO_API AssetManager
    {
      public:
        using Dependencies = TypeList<AssetSource, AssetDecoder>;

        explicit inline AssetManager(AssetSource &asset_source, std::vector<std::shared_ptr<AssetDecoder>> decoders)
            : asset_source_{std::addressof(asset_source)}, decoders_{std::move(decoders)}
        {
        }

        template <std::derived_from<Asset> T = Asset>
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
                        auto cast_ptr = dynamic_pointer_cast<T>(std::move(asset));
                        if (cast_ptr == nullptr)
                        {
                            return std::unexpected{AssetLoadError::AssetTypeMismatch};
                        }

                        return std::move(cast_ptr);
                    });
            }
        }

        bool remove_asset_from_cache(const AssetPath &path);

      private:
        AssetLoadResult<RefCountPtr<Asset>> load_asset_internal(const AssetPath &path);
        AssetLoadResult<RefCountPtr<Asset>> load_asset_from_stream(const AssetPath &path, Stream &stream);

        AssetSource *asset_source_{};
        std::vector<std::shared_ptr<AssetDecoder>> decoders_;
        std::shared_mutex asset_cache_mutex_{};
        std::unordered_map<AssetPath, Asset *> asset_cache_{};
    };
} // namespace retro
