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

    template <typename>
    struct FunctionTraits;

    template <typename R, typename... Args>
    struct FunctionTraits<R(Args...)>
    {
        using ReturnType = R;
        using ArgsTuple = std::tuple<Args...>;
        static constexpr std::size_t arity = sizeof...(Args);

        template <std::size_t I>
        using Arg = std::tuple_element_t<I, ArgsTuple>;
    };

    template <typename R, typename... Args>
    struct FunctionTraits<R (*)(Args...)> : FunctionTraits<R(Args...)>
    {
    };

    template <typename R, typename Class, typename... Args>
    struct FunctionTraits<R (Class::*)(Args...)> : FunctionTraits<R(Args...)>
    {
        using ClassType = Class;
    };

    template <typename R, typename Class, typename... Args>
    struct FunctionTraits<R (Class::*)(Args...) const> : FunctionTraits<R(Args...)>
    {
        using ClassType = Class;
    };

    template <typename R, typename Class, typename... Args>
    struct FunctionTraits<R (Class::*)(Args...) noexcept> : FunctionTraits<R(Args...)>
    {
        using ClassType = Class;
    };

    template <typename R, typename Class, typename... Args>
    struct FunctionTraits<R (Class::*)(Args...) const noexcept> : FunctionTraits<R(Args...)>
    {
        using ClassType = Class;
    };

    template <typename T>
    concept HasNonGenericCallOperator = requires { &std::remove_reference_t<T>::operator(); };

    template <HasNonGenericCallOperator T>
    struct FunctionTraits<T> : FunctionTraits<decltype(&std::remove_reference_t<T>::operator())>
    {
    };

    export template <typename T>
    using FunctionReturnType = FunctionTraits<T>::ReturnType;

    export template <typename T>
    using FunctionArgsTuple = FunctionTraits<T>::ArgsTuple;

    export template <typename T>
    constexpr std::size_t function_arity = FunctionTraits<T>::arity;

    export template <typename T, std::size_t I>
    using FunctionArg = FunctionTraits<T>::template Arg<I>;

    export template <typename T>
    using FunctionClassType = FunctionTraits<T>::ClassType;

    export template <typename T>
    concept FunctionType = requires {
        typename FunctionReturnType<T>;
        typename FunctionArgsTuple<T>;
        {
            function_arity<T>
        } -> std::convertible_to<std::size_t>;
    };

    export template <typename T>
    concept FreeFunction = FunctionType<T> && !requires { typename FunctionClassType<T>; };

    export template <typename T>
    concept MemberFunction = FunctionType<T> && requires { typename FunctionClassType<T>; };

    export template <typename T>
    concept NonGenericLambda =
        HasNonGenericCallOperator<T> && MemberFunction<T> && std::same_as<std::remove_cvref_t<T>, FunctionClassType<T>>;
} // namespace retro
