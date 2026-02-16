/**
 * @file localized_text_source.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.localized_text_source;

import std;
import retro.core.containers.optional;
import retro.core.util.enum_class_flags;
import retro.core.functional.delegate;
import retro.core.localization.text_data;
import retro.core.localization.text_key;
import retro.core.localization.locale;

namespace retro
{
    export enum class LocalizedTextSourceCategory : std::uint8_t
    {
        game,
        engine,
        editor
    };

    struct LocalizedTextSourcePriority
    {
        static constexpr std::int32_t lowest = -1000;
        static constexpr std::int32_t low = -100;
        static constexpr std::int32_t normal = 0;
        static constexpr std::int32_t high = 100;
        static constexpr std::int32_t highest = 1000;
    };

    using LocalizedStringResult = Optional<LocalizedStringConstPtr>;

    export class LocalizedTextSource
    {
      public:
        virtual ~LocalizedTextSource() = default;

        [[nodiscard]] virtual std::int32_t priority() const
        {
            return LocalizedTextSourcePriority::normal;
        }

        virtual Optional<std::u16string> get_native_culture_name(LocalizedTextSourceCategory category) = 0;

        virtual std::unordered_set<std::u16string> get_localized_culture_names(
            LocalizedTextSourceCategory category) = 0;

        [[nodiscard]] inline LocalizedStringResult get_localized_string(const TextId text_id, LocalePtr locale) const
        {
            return get_localized_string(text_id, std::move(locale), {});
        }

        [[nodiscard]] virtual LocalizedStringResult get_localized_string(TextId text_id,
                                                                         LocalePtr locale,
                                                                         std::u16string_view fallback_source) const = 0;
    };
} // namespace retro
