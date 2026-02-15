/**
 * @file localized_string.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization;

import :localized_string;
import :localization_manager;

namespace retro
{
    class UnlocalizedString final : public LocalizedString
    {
      public:
        explicit UnlocalizedString(std::u16string source, const LocalizedStringFlags flags = LocalizedStringFlags::none)
            : source_{std::move(source)}, flags_{flags}
        {
        }

        const std::u16string &source_string() const noexcept override
        {
            return source_;
        }

        const std::u16string &display_string() const noexcept override
        {
            return source_;
        }

        TextRevision revision() const noexcept override
        {
            return TextRevision{0, 0};
        }

        LocalizedStringFlags flags() const noexcept override
        {
            return flags_;
        }

      private:
        std::u16string source_;
        LocalizedStringFlags flags_;
    };

    LocalizedStringPtr make_unlocalized_string(std::u16string source, LocalizedStringFlags flags)
    {
        return make_ref_counted<UnlocalizedString>(std::move(source), flags);
    }
} // namespace retro
