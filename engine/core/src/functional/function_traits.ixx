/**
 * @file function_traits.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:functional.function_traits;

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

    export template <typename T>
    concept CallableObject = IsCallable<T>::value;
} // namespace retro
