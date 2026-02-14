/**
 * @file text_source_types.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization:text_source_types;

import std;

import retro.core.util.enum_class_flags;

namespace retro
{
    export enum class LocalizedTextSourceCategory : std::uint8_t
    {
        game,
        engine,
        editor
    };

    export enum class QueryLocalizedResourceResult : std::uint8_t
    {
        found,
        not_found,
        not_implemented
    };

    export enum class LocalizationLoadFlags : std::uint8_t
    {
        none = 0,
        native = 1 << 0,
        editor = 1 << 1,
        game = 1 << 2,
        engine = 1 << 3,
        additional = 1 << 4,
        force_localized_game = 1 << 5,
        skip_existing = 1 << 6
    };

    export template <>
    constexpr bool is_flag_enum<LocalizationLoadFlags> = true;

    struct LocalizedTextSourcePriority
    {
        static constexpr std::int32_t lowest = -1000;
        static constexpr std::int32_t low = -100;
        static constexpr std::int32_t normal = 0;
        static constexpr std::int32_t high = 100;
        static constexpr std::int32_t highest = 1000;
    };

    class DisplayString final
    {
      public:
        explicit constexpr DisplayString(std::u16string string) : string_(std::move(string))
        {
        }

        [[nodiscard]] constexpr const std::u16string &string() const noexcept
        {
            return string_;
        }

      private:
        std::u16string string_;
    };

    export using TextDisplayStringPtr = std::shared_ptr<DisplayString>;
    export using TextConstDisplayStringPtr = std::shared_ptr<const DisplayString>;

    export inline TextDisplayStringPtr make_text_display_string(std::u16string string)
    {
        return std::make_shared<DisplayString>(std::move(string));
    }
} // namespace retro
