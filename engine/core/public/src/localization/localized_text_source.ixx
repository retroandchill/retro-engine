/**
 * @file localized_text_source.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization:localized_text_source;

import std;
import retro.core.containers.optional;
import retro.core.util.enum_class_flags;

namespace retro
{
    export enum class LocalizedTextSourceCategory : std::uint8_t
    {
        game,
        engine,
        editor
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

        static inline bool should_load_native(const LocalizationLoadFlags load_flags)
        {
            return has_any_flags(load_flags, LocalizationLoadFlags::native);
        }

        static inline bool should_load_editor(const LocalizationLoadFlags load_flags)
        {
            return has_any_flags(load_flags, LocalizationLoadFlags::editor);
        }

        static inline bool should_load_game(const LocalizationLoadFlags load_flags)
        {
            return has_any_flags(load_flags, LocalizationLoadFlags::game | LocalizationLoadFlags::force_localized_game);
        }

        static inline bool should_load_additional(const LocalizationLoadFlags load_flags)
        {
            return has_any_flags(load_flags, LocalizationLoadFlags::additional);
        }

        static inline bool should_load_native_game_data(const LocalizationLoadFlags load_flags)
        {
#if RETRO_WITH_EDITOR_DATA
            return !has_all_flags(load_flags, LocalizationLoadFlags::force_localized_game);
#else
            return false;
#endif
        }
    };
} // namespace retro
