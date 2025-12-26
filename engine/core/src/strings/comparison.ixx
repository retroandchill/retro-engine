//
// Created by fcors on 12/24/2025.
//

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