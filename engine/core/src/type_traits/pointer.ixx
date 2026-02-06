/**
 * @file pointer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.pointer;

import std;
import retro.core.type_traits.basic;
import retro.core.memory.ref_counted_ptr;

namespace retro
{
    export template <typename T>
    concept NothrowDereferenceable = noexcept(*std::declval<T>());

    template <typename>
    struct IsStdUniquePtr : std::false_type
    {
    };

    template <typename T>
    struct IsStdUniquePtr<std::unique_ptr<T>> : std::true_type
    {
    };

    export template <typename T>
    concept UniquePtrLike = IsStdUniquePtr<std::remove_cvref_t<T>>::value;

    template <typename T>
    struct IsStdSharedPtr : std::false_type
    {
    };

    template <typename T>
    struct IsStdSharedPtr<std::shared_ptr<T>> : std::true_type
    {
    };

    export template <typename T>
    concept SharedPtrLike = IsStdSharedPtr<std::remove_cvref_t<T>>::value;

    export template <typename T>
    struct IsStdWeakPtr : std::false_type
    {
    };

    template <typename U>
    struct IsStdWeakPtr<std::weak_ptr<U>> : std::true_type
    {
    };

    export template <typename T>
    concept WeakPtrLike = IsStdWeakPtr<std::remove_cvref_t<T>>::value;

    export template <typename T>
    concept WeakFromThisCapable = requires(T &t) {
        {
            t.weak_from_this()
        } -> WeakPtrLike;
    };

    export template <typename T>
    concept WeakBindable = SharedPtrLike<T> || WeakPtrLike<T> || WeakFromThisCapable<T>;

    export constexpr auto to_weak(SharedPtrLike auto const &sp)
    {
        return std::weak_ptr{sp};
    }

    export template <WeakPtrLike T>
    constexpr decltype(auto) to_weak(T &&wp)
    {
        return std::forward<T>(wp);
    }

    export constexpr auto to_weak(WeakFromThisCapable auto &obj)
    {
        return obj.weak_from_this();
    }

    template <typename>
    struct IsRefCountPtr : std::false_type
    {
    };

    template <typename T>
    struct IsRefCountPtr<RefCountPtr<T>> : std::true_type
    {
    };

    export template <typename T>
    concept RefCountPtrLike = IsRefCountPtr<std::remove_cvref_t<T>>::value;

    export template <typename T>
    struct PointerElement;

    template <typename T>
    struct PointerElement<T *>
    {
        using Type = T;
    };

    template <typename T>
    struct PointerElement<std::unique_ptr<T>>
    {
        using Type = T;
    };

    template <typename T>
    struct PointerElement<std::shared_ptr<T>>
    {
        using Type = T;
    };

    template <typename T>
    struct PointerElement<std::weak_ptr<T>>
    {
        using Type = T;
    };

    template <typename T>
    struct PointerElement<RefCountPtr<T>>
    {
        using Type = T;
    };

    export template <typename T>
    using PointerElementT = PointerElement<std::remove_cvref_t<T>>::Type;

    export template <typename From, typename To>
    concept PointerConvertible =
        (ProperBaseOf<To, From> && std::has_virtual_destructor_v<To> && !LessCvQualified<To, From>) ||
        (SameUnqualifed<From, To> && !LessCvQualified<To, From>);
} // namespace retro
