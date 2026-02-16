/**
 * @file text_transformer.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization.text_transformer;

import retro.core.localization.localization_manager;

namespace retro
{
    std::u16string TextTransformer::to_lower(std::u16string_view text)
    {
        return LocalizationManager::get().current_locale()->to_lower(text);
    }

    std::u16string TextTransformer::to_upper(std::u16string_view text)
    {
        return LocalizationManager::get().current_locale()->to_upper(text);
    }
} // namespace retro
