//
// Created by fcors on 12/24/2025.
//

export module retro.core.strings:comparison;

import retro.core;

namespace retro::core {
    export enum class StringComparison : uint8 {
        CaseSensitive,
        CaseInsensitive
    };
}