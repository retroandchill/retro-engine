/**
 * @file deferred.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.util.deferred;

import std;

namespace retro
{
    export template <std::invocable Functor>
    class Deferred
    {
      public:
        template <typename... Args>
            requires std::constructible_from<Functor, Args...>
        explicit constexpr Deferred(Args &&...args) : functor_{std::forward<Args>(args)...}
        {
        }

        Deferred(const Deferred &) = delete;
        Deferred(Deferred &&) noexcept = delete;

        ~Deferred()
        {
            functor_();
        }

        Deferred &operator=(const Deferred &) = delete;
        Deferred &operator=(Deferred &&) noexcept = delete;

      private:
        Functor functor_;
    };

    export template <std::invocable Functor>
    Deferred(Functor &&) -> Deferred<std::decay_t<Functor>>;
} // namespace retro
