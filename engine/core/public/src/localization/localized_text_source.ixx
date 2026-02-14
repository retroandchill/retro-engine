/**
 * @file localized_text_source.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization:localized_text_source;

import std;
import :text_source_types;
import retro.core.containers.optional;

namespace retro
{
    export class LocalizedTextSource
    {
      public:
        virtual ~LocalizedTextSource() = default;

        [[nodiscard]] virtual std::int32_t priority() const
        {
            return LocalizedTextSourcePriority::normal;
        }

        virtual Optional<std::u16string> query_localized_string(LocalizedTextSourceCategory category,
                                                                std::u16string_view source_string) = 0;

        virtual Optional<std::u16string> get_native_culture_name(LocalizedTextSourceCategory category) = 0;

        virtual std::unordered_set<std::u16string> get_available_cultures(LocalizedTextSourceCategory category) = 0;
    };
} // namespace retro
