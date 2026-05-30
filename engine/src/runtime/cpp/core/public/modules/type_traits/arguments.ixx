/**
 * @file arguments.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.arguments;

import std;

namespace retro
{
    export template <typename... Args>
    struct FirstArgument;

    template <typename First, typename... Rest>
    struct FirstArgument<First, Rest...>
    {
        using Type = First;
    };

    export template <typename... Args>
    using FirstArgumentType = FirstArgument<Args...>::Type;

    export template <typename... Args>
    struct LastArgument;

    template <typename Last>
    struct LastArgument<Last>
    {
        using Type = Last;
    };

    template <typename First, typename... Rest>
    struct LastArgument<First, Rest...>
    {
        using Type = LastArgument<Rest...>::Type;
    };

    export template <typename... Args>
    using LastArgumentType = LastArgument<Args...>::Type;

    export template <typename... Args>
    concept HasArguments = sizeof...(Args) > 0;

    export template <typename...>
    struct TypeList
    {
    };

    export template <typename List>
    struct DropFirstArgument;

    template <typename First, typename... Rest>
    struct DropFirstArgument<TypeList<First, Rest...>>
    {
        using Type = TypeList<Rest...>;
    };

    export template <typename List>
    struct DropLastArgument;

    template <typename Last>
    struct DropLastArgument<TypeList<Last>>
    {
        using Type = TypeList<>;
    };

    template <typename First, typename... Rest>
    struct DropLastArgument<TypeList<First, Rest...>>
    {
      private:
        using Tail = DropLastArgument<TypeList<Rest...>>::Type;

        template <typename... TailArgs>
        static TypeList<First, TailArgs...> prepend(TypeList<TailArgs...>);

      public:
        using Type = decltype(prepend(std::declval<Tail>()));
    };

    template <typename Functor, typename List>
    struct IsInvocableWithList;

    template <typename Functor, typename... Args>
    struct IsInvocableWithList<Functor, TypeList<Args...>> : std::bool_constant<std::invocable<Functor, Args...>>
    {
        static constexpr bool nothrow = std::is_nothrow_invocable_v<Functor, Args...>;
    };

    template <typename Functor, typename List>
    struct ListInvocationResult;

    template <typename Functor, typename... Args>
        requires std::invocable<Functor, Args...>
    struct ListInvocationResult<Functor, TypeList<Args...>>
    {
        using Type = std::invoke_result_t<Functor, Args...>;
    };

    export template <typename Functor, typename List>
    concept InvocableWithList = IsInvocableWithList<Functor, List>::value;

    export template <typename Functor, typename List>
    concept NothrowInvocableWithList = IsInvocableWithList<Functor, List>::nothrow;

    export template <typename Functor, typename List>
    using InvokeWithListResult = ListInvocationResult<Functor, List>::Type;

    export template <typename Functor, typename... Args>
    concept InvocableWithoutLastArg = InvocableWithList<Functor, typename DropLastArgument<TypeList<Args...>>::Type>;

    export template <typename Functor, typename... Args>
    concept NothrowInvocableWithoutLastArg =
        InvocableWithList<Functor, typename DropLastArgument<TypeList<Args...>>::Type>;

    export template <typename Functor, typename... Args>
        requires InvocableWithoutLastArg<Functor, Args...>
    using InvokeWithoutLastArgResult =
        InvokeWithListResult<Functor, typename DropLastArgument<TypeList<Args...>>::Type>;

    export template <typename Functor, typename... Args>
    concept InvocableWithoutFirstArg = InvocableWithList<Functor, typename DropFirstArgument<TypeList<Args...>>::Type>;

    export template <typename Functor, typename... Args>
    concept NothrowInvocableWithoutFirstArg =
        NothrowInvocableWithList<Functor, typename DropFirstArgument<TypeList<Args...>>::Type>;

    export template <typename Functor, typename... Args>
        requires InvocableWithoutFirstArg<Functor, Args...>
    using InvokeWithoutFirstArgResult =
        InvokeWithListResult<Functor, typename DropFirstArgument<TypeList<Args...>>::Type>;

    export template <typename Functor, typename... Args>
        requires InvocableWithoutFirstArg<Functor, Args...>
    constexpr decltype(auto) invoke_without_first_arg(Functor &&functor, Args &&...args) noexcept(
        NothrowInvocableWithoutFirstArg<Functor, Args...>)
    {
        auto tuple = std::forward_as_tuple(std::forward<Args>(args)...);

        return [&]<std::size_t... Indices>(std::index_sequence<Indices...>)
        {
            return std::invoke(std::forward<Functor>(functor), std::get<Indices + 1>(std::move(tuple))...);
        }(std::make_index_sequence<sizeof...(Args) - 1>{});
    }

    export template <typename Functor, typename... Args>
        requires InvocableWithoutLastArg<Functor, Args...>
    constexpr decltype(auto) invoke_without_last_arg(Functor &&functor, Args &&...args) noexcept(
        NothrowInvocableWithoutFirstArg<Functor, Args...>)
    {
        auto tuple = std::forward_as_tuple(std::forward<Args>(args)...);

        return [&]<std::size_t... Indices>(std::index_sequence<Indices...>)
        {
            return std::invoke(std::forward<Functor>(functor), std::get<Indices>(std::move(tuple))...);
        }(std::make_index_sequence<sizeof...(Args) - 1>{});
    }
} // namespace retro
