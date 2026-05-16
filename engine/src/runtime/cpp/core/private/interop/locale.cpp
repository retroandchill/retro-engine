/**
 * @file locale.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <unicode/urename.h>

import std;
import retro.core.localization.icu;
import retro.core.localization.buffers;

extern "C"
{
    RETRO_API const icu::Locale *retro_get_default_locale()
    {
        return &icu::Locale::getDefault();
    }

    RETRO_API icu::Locale *retro_create_default_locale()
    {
        return new icu::Locale();
    }

    RETRO_API icu::Locale *retro_create_locale(const char *locale)
    {
        return new icu::Locale(locale);
    }

    RETRO_API void retro_destroy_locale(const icu::Locale *locale)
    {
        delete locale;
    }

    RETRO_API std::uint8_t retro_locale_is_bogus(const icu::Locale *locale)
    {
        return locale->isBogus();
    }

    RETRO_API const char *retro_locale_get_name(const icu::Locale *locale)
    {
        return locale->getName();
    }

    RETRO_API std::int32_t retro_locale_get_display_name(const icu::Locale *locale,
                                                         char16_t *buffer,
                                                         std::int32_t length)
    {
        icu::UnicodeString str;
        locale->getDisplayName(str);
        return retro::write_to_output_buffer(str, std::span(buffer, length));
    }

    RETRO_API std::int32_t retro_locale_get_english_name(const icu::Locale *locale,
                                                         char16_t *buffer,
                                                         std::int32_t length)
    {
        icu::UnicodeString str;
        locale->getDisplayName(icu::Locale("en"), str);
        return retro::write_to_output_buffer(str, std::span(buffer, length));
    }

    RETRO_API const char *retro_locale_get_three_letter_language_name(const icu::Locale *locale)
    {
        return locale->getISO3Language();
    }

    RETRO_API const char *retro_locale_get_two_letter_language_name(const icu::Locale *locale)
    {
        return locale->getLanguage();
    }

    RETRO_API const char *retro_locale_get_region(const icu::Locale *locale)
    {
        return locale->getCountry();
    }

    RETRO_API const char *retro_locale_get_script(const icu::Locale *locale)
    {
        return locale->getScript();
    }

    RETRO_API const char *retro_locale_get_variant(const icu::Locale *locale)
    {
        return locale->getVariant();
    }

    RETRO_API std::int32_t retro_locale_get_display_language(const icu::Locale *locale,
                                                             char16_t *buffer,
                                                             const std::int32_t length)
    {
        icu::UnicodeString str;
        locale->getDisplayLanguage(str);
        return retro::write_to_output_buffer(str, std::span(buffer, length));
    }

    RETRO_API std::int32_t retro_locale_get_display_region(const icu::Locale *locale,
                                                           char16_t *buffer,
                                                           const std::int32_t length)
    {
        icu::UnicodeString str;
        locale->getDisplayCountry(str);
        return retro::write_to_output_buffer(str, std::span(buffer, length));
    }

    RETRO_API std::int32_t retro_locale_get_display_script(const icu::Locale *locale,
                                                           char16_t *buffer,
                                                           const std::int32_t length)
    {
        icu::UnicodeString str;
        locale->getDisplayScript(str);
        return retro::write_to_output_buffer(str, std::span(buffer, length));
    }

    RETRO_API std::int32_t retro_locale_get_display_variant(const icu::Locale *locale,
                                                            char16_t *buffer,
                                                            const std::int32_t length)
    {
        icu::UnicodeString str;
        locale->getDisplayVariant(str);
        return retro::write_to_output_buffer(str, std::span(buffer, length));
    }

    RETRO_API std::uint8_t retro_locale_is_right_to_left(const icu::Locale *locale)
    {
        return locale->isRightToLeft();
    }

    RETRO_API std::uint32_t retro_locale_get_lcid(const icu::Locale *locale)
    {
        return locale->getLCID();
    }

    RETRO_API const icu::Locale *retro_get_available_locales(std::int32_t *count, std::int32_t *stride)
    {
        *stride = sizeof(icu::Locale);
        return icu::Locale::getAvailableLocales(*count);
    }

    RETRO_API const char *const *retro_locale_get_available_languages()
    {
        return icu::Locale::getISOLanguages();
    }

    RETRO_API void retro_locale_set_default(const char *locale_name)
    {
        UErrorCode status;
        uloc_setDefault(locale_name, &status);
    }
}
