/**
 * @file binding.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.functional.binding;

import retro.core.type_traits.callable;
import std;

namespace retro
{
    template <auto Functor>
        requires CallableObject<decltype(Functor)>
    struct ConstantBinding
    {
        template <typename... Args>
            requires std::invocable<decltype(Functor), Args...>
        constexpr decltype(auto) operator()(Args &&...args) const
            noexcept(std::is_nothrow_invocable_v<decltype(Functor), Args...>)
        {
            return std::invoke(Functor, std::forward<Args>(args)...);
        }
    };

    export template <auto Functor, typename... Args>
    constexpr auto bind_front(Args &&...args)
    {
        return std::bind_front(ConstantBinding<Functor>{}, std::forward<Args>(args)...);
    }

    export template <auto Functor, typename... Args>
    constexpr auto bind_back(Args &&...args)
    {
        return std::bind_back(ConstantBinding<Functor>{}, std::forward<Args>(args)...);
    }
} // namespace retro
