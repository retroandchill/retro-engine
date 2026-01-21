/**
 * @file concepts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:concepts;

import std;

namespace retro
{
    export template <typename From, typename To>
    concept CanStaticCast = requires(From &&from) {
        {
            static_cast<To>(std::forward<From>(from))
        } -> std::same_as<To>;
    };

    /**
     * Concept to ensure that the underlying type is a character.
     */
    export template <typename T>
    concept Char = std::same_as<T, char> || std::same_as<T, wchar_t> || std::same_as<T, char8_t> ||
                   std::same_as<T, char16_t> || std::same_as<T, char32_t>;

    export template <typename T>
    concept Numeric =
        std::same_as<T, std::remove_cvref_t<T>> && std::is_arithmetic_v<T> && !std::same_as<T, bool> && !Char<T>;

    export template <typename T, typename Variant>
    struct IsVariantMember : std::false_type
    {
    };

    template <typename T, typename... Types>
    struct IsVariantMember<T, std::variant<Types...>> : std::bool_constant<(std::is_same_v<T, Types> || ...)>
    {
    };

    export template <typename T, typename Variant>
    concept VariantMember = IsVariantMember<T, Variant>::value;

    export template <typename Functor, typename Variant>
    concept CanVisitVariant = requires(Variant &variant, Functor functor) {
        {
            std::visit(functor, variant)
        };
    };

    export template <typename Functor, typename Variant, typename ReturnType>
    concept CanVisitVariantReturning = requires(Variant &variant, Functor functor) {
        {
            std::visit(functor, variant)
        } -> std::convertible_to<ReturnType>;
    };

    /**
     * Public export of the concept for the minimum requirements for a type to be a valid allocator
     * as published in the C++26 standard.
     *
     * @tparam Alloc The allocator type under test.
     */
    export template <class Alloc>
    concept SimpleAllocator = requires(Alloc alloc, std::size_t n) {
        // ReSharper disable once CppRedundantTypenameKeyword
        {
            *alloc.allocate(n)
        } -> std::same_as<typename Alloc::value_type &>;
        {
            alloc.deallocate(alloc.allocate(n), n)
        };
    } && std::copy_constructible<Alloc> && std::equality_comparable<Alloc>;

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
