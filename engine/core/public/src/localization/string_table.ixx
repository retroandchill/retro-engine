/**
 * @file string_table.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.string_table;

import std;

namespace retro
{
    export enum class StringTableLoadingPolicy : std::uint8_t
    {
        find,
        find_or_load,
        find_or_fully_load
    };
} // namespace retro
