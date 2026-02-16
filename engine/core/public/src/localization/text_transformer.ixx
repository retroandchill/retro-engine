/**
 * @file text_transformer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization.text_transformer;

import std;

namespace retro
{
    export class RETRO_API TextTransformer
    {
      public:
        static std::u16string to_lower(std::u16string_view text);
        static std::u16string to_upper(std::u16string_view text);
    };
} // namespace retro
