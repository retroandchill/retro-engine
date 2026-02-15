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
    export namespace text
    {
        inline std::size_t hash_string(const std::u16string_view str)
        {
            return std::hash<std::u16string_view>{}(str);
        }

        inline std::size_t hash_string(const std::u16string_view str, const std::uint32_t base)
        {
            return hash_combine(base, str);
        }
    } // namespace text

    export class RETRO_API TextKey
    {
      public:
        constexpr TextKey() = default;

        explicit TextKey(std::u16string_view key) noexcept;

        [[nodiscard]] std::u16string to_string() const;
        void append_string(std::u16string &out) const;

        constexpr friend bool operator==(const TextKey &lhs, const TextKey &rhs) noexcept = default;

        [[nodiscard]] constexpr bool is_empty() const noexcept
        {
            return index_ == 0;
        }

        constexpr void reset()
        {
            index_ = 0;
        }

      private:
        explicit constexpr TextKey(const std::int32_t index) noexcept : index_{index}
        {
        }

        std::int32_t index_ = 0;

        friend std::hash<TextKey>;
        friend class TextKeyState;
    };

    export class TextId
    {
      public:
        constexpr TextId() = default;

        constexpr TextId(const TextKey ns, const TextKey key) noexcept : namespace_{ns}, key_{key}
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

        constexpr friend bool operator==(const TextId &lhs, const TextId &rhs)
        {
            return lhs.namespace_ == rhs.namespace_ && lhs.key_ == rhs.key_;
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

      private:
        friend std::hash<TextId>;

        TextKey namespace_;
        TextKey key_;
    };
} // namespace retro

template <>
struct std::hash<retro::TextKey>
{
    RETRO_API std::size_t operator()(const retro::TextKey &key) const noexcept;
};

template <>
struct std::hash<retro::TextId>
{
    std::size_t operator()(const retro::TextId &id) const noexcept
    {
        return hash_combine(id.namespace_, id.key_);
    }
};
