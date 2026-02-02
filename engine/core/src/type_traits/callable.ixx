/**
 * @file callable.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.callable;

import std;

namespace retro
{
    template <typename T>
    struct IsCallable
    {
      private:
        using yes = char (&)[1];
        using no = char (&)[2];

        struct Fallback
        {
            void operator()();
        };

        struct Derived : T, Fallback
        {
        };

        template <typename U, U>
        struct Check;

        template <typename>
        static yes test(...);

        template <typename C>
        static no test(Check<void (Fallback::*)(), &C::operator()> *);

      public:
        static const bool value = sizeof(test<Derived>(nullptr)) == sizeof(yes);
    };

    template <typename T>
        requires std::is_function_v<std::remove_pointer_t<T>>
    struct IsCallable<T> : std::true_type
    {
    };

    template <typename T>
        requires std::is_member_pointer_v<T>
    struct IsCallable<T> : std::true_type
    {
    };

    export template <typename T>
    concept CallableObject = IsCallable<T>::value;
} // namespace retro
