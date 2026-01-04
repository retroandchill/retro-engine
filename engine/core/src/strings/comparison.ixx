/**
 * @file comparison.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:strings.comparison;

import :defines;

namespace retro
{
    export enum class StringComparison : uint8
    {
        CaseSensitive,
        CaseInsensitive
    };
} // namespace retro
