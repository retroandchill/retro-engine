/**
 * @file culture_info.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <unicode/locid.h>

export module retro.core.localization.culture_info;

import std;
import retro.core.strings.cstring_view;
import retro.core.util.noncopyable;

namespace retro
{
    export class RETRO_API CultureInfo : NonCopyable
    {
        struct ConstructTag
        {
        };

      public:
        explicit CultureInfo(ConstructTag, CStringView locale_tag = "");

        static inline std::shared_ptr<CultureInfo> create(CStringView locale_tag = "")
        {
            return std::make_shared<CultureInfo>(ConstructTag{}, locale_tag);
        }

        // Locale identification
        [[nodiscard]] inline const std::string &locale_tag() const noexcept
        {
            return locale_tag_;
        }
        [[nodiscard]] inline const std::string &name() const noexcept
        {
            return name_;
        }
        [[nodiscard]] inline const std::string &english_name() const noexcept
        {
            return english_name_;
        }

        // Language and region
        [[nodiscard]] inline const std::string &language() const noexcept
        {
            return language_;
        }

        [[nodiscard]] inline const std::string &region() const noexcept
        {
            return region_;
        }

        // Text properties
        [[nodiscard]] inline bool is_right_to_left() const noexcept
        {
            return is_right_to_left_;
        }

        friend bool operator==(const CultureInfo &lhs, const CultureInfo &rhs) noexcept;

      private:
        std::string locale_tag_;
        icu::Locale icu_locale_;
        std::string name_;
        std::string english_name_;
        std::string language_;
        std::string region_;
        std::string script_;
        bool is_right_to_left_ = false;
    };

    export using CultureInfoPtr = std::shared_ptr<CultureInfo>;
} // namespace retro
