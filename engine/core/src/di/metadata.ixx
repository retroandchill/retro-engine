/**
 * @file metadata.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:metadata;

import std;

namespace retro
{
    export template <typename...>
    struct TypeList
    {
    };

    template <typename List, typename T>
    struct PushBack;

    template <typename... Ts, typename T>
    struct PushBack<TypeList<Ts...>, T>
    {
        using Type = TypeList<Ts..., T>;
    };

    template <typename A, typename B>
    struct Concat;

    template <typename... As, typename... Bs>
    struct Concat<TypeList<As...>, TypeList<Bs...>>
    {
        using Type = TypeList<As..., Bs...>;
    };

    template <typename>
    struct IsTypeList : std::false_type
    {
    };

    template <typename... Ts>
    struct IsTypeList<TypeList<Ts...>> : std::true_type
    {
    };

    template <typename T>
    concept ValidTypeList = IsTypeList<T>::value;

    template <typename Functor, typename... Deps>
    concept CallableOn = requires(Functor functor) { functor.template operator()<Deps...>(); };

    template <typename>
    struct TypeListApply;

    template <typename... Deps>
    struct TypeListApply<TypeList<Deps...>>
    {
        template <CallableOn<Deps...> Functor>
        static decltype(auto) call(Functor &&functor)
        {
            return std::forward<Functor>(functor).template operator()<Deps...>();
        }
    };

    export template <typename T>
    concept HasDependencies = requires { typename T::Dependencies; } && ValidTypeList<typename T::Dependencies>;

    export template <HasDependencies T>
    using DependenciesOf = T::Dependencies;

    template <typename D>
    using ArgChoices = TypeList<D &, std::vector<D *>, const std::vector<D *> &, D>;

    template <typename T, typename ArgList>
    struct IsConstructibleFromList;

    template <typename T, typename... Args>
    struct IsConstructibleFromList<T, TypeList<Args...>> : std::bool_constant<std::constructible_from<T, Args...>>
    {
    };

    using NotFound = void;

    template <typename T, typename Deps, typename Picked>
    struct FindFistCtorArgs;

    template <typename T, typename Picked>
    struct FindFistCtorArgs<T, TypeList<>, Picked>
    {
        using Type = std::conditional_t<IsConstructibleFromList<T, Picked>::value, Picked, NotFound>;
    };

    template <typename T, typename D0, typename... RestDeps, typename Picked>
    struct FindFistCtorArgs<T, TypeList<D0, RestDeps...>, Picked>
    {
      private:
        template <typename A>
        using TryA = FindFistCtorArgs<T, TypeList<RestDeps...>, typename PushBack<Picked, A>::Type>::Type;

        template <typename Choices>
        struct TryChoicesRec;

        template <>
        struct TryChoicesRec<TypeList<>>
        {
            using Type = NotFound;
        };

        template <typename A0, typename... As>
        struct TryChoicesRec<TypeList<A0, As...>>
        {
            using Attempt = TryA<A0>;
            using Type = std::
                conditional_t<std::same_as<Attempt, NotFound>, typename TryChoicesRec<TypeList<As...>>::Type, Attempt>;
        };

      public:
        using Type = TryChoicesRec<ArgChoices<D0>>::Type;
    };

    export template <HasDependencies T>
    using SelectedCtorArgs = FindFistCtorArgs<T, DependenciesOf<T>, TypeList<>>::Type;

    template <typename T>
    concept ConstructorDependenciesValid = HasDependencies<T> && requires { typename SelectedCtorArgs<T>; };

    export template <typename T>
    concept Injectable = ConstructorDependenciesValid<T> || std::is_default_constructible_v<T>;

    template <typename T>
    concept HasCType = requires() { typename T::CType; };

    export template <typename T>
    struct HandleTraits;

    export template <HasCType T>
    struct HandleTraits<T>
    {
        using HandleType = T::CType;
    };

    template <typename T>
    concept HasHandleType = requires { typename HandleTraits<std::remove_cvref_t<T>>::HandleType; };

    export template <HasHandleType T>
    using HandleType = HandleTraits<std::remove_cvref_t<T>>::HandleType;

    export template <typename T>
    concept HandleWrapper = HasHandleType<T> && std::is_pointer_v<HandleType<T>> &&
                            std::convertible_to<HandleType<T>, std::remove_cvref_t<T>> &&
                            std::convertible_to<std::remove_cvref_t<T>, HandleType<T>>;

    export template <typename T>
    concept SmartHandle = requires(T ptr) {
        {
            ptr.get()
        } -> HandleWrapper;
    };

    export template <SmartHandle T>
    using HandleElementType = std::remove_cvref_t<decltype(std::declval<T>().get())>;
} // namespace retro
