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

    template <typename T>
    concept ConstructorDependenciesValid = HasDependencies<T> && IsConstructibleFromList<T, DependenciesOf<T>>::value;

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
