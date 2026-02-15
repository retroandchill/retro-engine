/**
 * @file text_key.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization:text_key;

import std;
import retro.core.type_traits.range;
import retro.core.algorithm.hashing;

namespace retro
{
    export class TextId;

    export class RETRO_API TextKey
    {
      public:
        constexpr TextKey() noexcept = default;

        explicit TextKey(std::u16string_view key) noexcept;

        [[nodiscard]] constexpr std::uint32_t id() const noexcept
        {
            return id_;
        }

        [[nodiscard]] const std::u16string &to_string() const noexcept;

        [[nodiscard]] constexpr bool is_empty() const noexcept
        {
            return id_ == 0;
        }

        constexpr void reset() noexcept
        {
            id_ = 0;
        }

        constexpr friend bool operator==(const TextKey &lhs, const TextKey &rhs) noexcept = default;

        constexpr friend std::strong_ordering operator<=>(const TextKey &lhs, const TextKey &rhs) noexcept = default;

      private:
        explicit constexpr TextKey(std::uint32_t id) noexcept : id_{id}
        {
        }

        std::uint32_t id_ = 0;

        friend std::hash<TextKey>;
        friend class TextId;
        friend class TextKeyRegistry;
    };

    class RETRO_API TextId
    {
      public:
        constexpr TextId() noexcept = default;

        constexpr TextId(const TextKey &ns, const TextKey &key) noexcept : namespace_{ns}, key_{key}
        {
        }

        [[nodiscard]] TextKey text_namespace() const noexcept
        {
            return namespace_;
        }

        [[nodiscard]] TextKey key() const noexcept
        {
            return key_;
        }

        [[nodiscard]] constexpr bool is_empty() const noexcept
        {
            return namespace_.is_empty() && key_.is_empty();
        }

        constexpr void reset() noexcept
        {
            namespace_.reset();
            key_.reset();
        }

        constexpr friend bool operator==(const TextId &lhs, const TextId &rhs) noexcept = default;

        constexpr friend std::strong_ordering operator<=>(const TextId &lhs, const TextId &rhs) noexcept = default;

      private:
        TextKey namespace_{};
        TextKey key_{};

        friend std::hash<TextId>;
        friend class TextKeyRegistry;
    };
} // namespace retro

template <>
struct std::hash<retro::TextKey>
{
    constexpr std::size_t operator()(const retro::TextKey &key) const noexcept
    {
        return retro::hash_combine(key.id());
    }
};

template <>
struct std::hash<retro::TextId>
{
    constexpr std::size_t operator()(const retro::TextId &id) const noexcept
    {
        return retro::hash_combine(id.namespace_, id.key_);
    }
};
