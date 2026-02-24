/**
 * @file buffers.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.buffers;

import retro.core.localization.icu;
import std;

namespace retro
{
    export inline std::int32_t write_to_output_buffer(const icu::UnicodeString &str, std::span<char16_t> buffer)
    {
        std::ranges::copy_n(str.begin(),
                            std::min(str.length(), static_cast<std::int32_t>(buffer.size())),
                            buffer.begin());
        return str.length();
    }
} // namespace retro
