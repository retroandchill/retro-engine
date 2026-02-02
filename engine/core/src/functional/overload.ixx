/**
 * @file overload.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.functional.overload;

import retro.core.type_traits.callable;

namespace retro
{
    export template <CallableObject... Ts>
    struct Overload : Ts...
    {
        using Ts::operator()...;
    };

    export template <CallableObject... Ts>
    Overload(Ts...) -> Overload<Ts...>;
} // namespace retro
