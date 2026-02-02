/**
 * @file c_api.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.c_api;

import std;

namespace retro
{
    /**
     * Defines what type of C-compatible handle we are creating.
     */
    export enum class CHandleType : std::uint8_t
    {
        /**
         * The handle is defined as an opaque struct and thus has no implementation and is safe to reinterpret_cast to
         * a pointer of any size.
         */
        Opaque,

        /**
         * The handle is defined as an actual struct and thus to convert between types they must have identical layout
         * and must be trivially copyable and destructible.
         */
        Defined
    };

    /**
     * Base traits type for defining a C-Handle type.
     *
     * @remarks While this can be directly defined it's better to use the DECLARE_OPAQUE_C_HANDLE and
     * DECLARE_DEFINED_C_HANDLE macros to do this instead.
     */
    export template <typename>
    struct CHandleTraits;

    export template <typename T>
    concept CHandle = requires {
        typename CHandleTraits<std::remove_cvref_t<T>>::CppType;
        {
            CHandleTraits<std::remove_cvref_t<T>>::HandleType
        } -> std::convertible_to<CHandleType>;
    };

    export template <CHandle T>
    using CppType = CHandleTraits<std::remove_cvref_t<T>>::CppType;

    export template <typename T>
    concept OpaqueHandle = CHandle<T> && CHandleTraits<std::remove_cvref_t<T>>::HandleType == CHandleType::Opaque;

    template <typename T>
    concept CompatibleCHandle =
        OpaqueHandle<T> || (sizeof(T) == sizeof(CppType<T>) && std::is_trivially_copyable_v<CppType<T>> &&
                            std::is_trivially_destructible_v<CppType<T>>);

    template <typename T>
    concept CompatibleCHandleReference =
        CompatibleCHandle<std::remove_reference_t<T>> && (!OpaqueHandle<T> || std::is_lvalue_reference_v<T>);

    export template <CompatibleCHandle T>
    [[nodiscard]] constexpr auto from_c(T *handle) noexcept
    {
        using Cpp = CppType<T>;
        using Result = std::conditional_t<std::is_const_v<T>, const Cpp *, Cpp *>;
        return reinterpret_cast<Result>(handle);
    }

    export template <CompatibleCHandleReference T>
    [[nodiscard]] constexpr decltype(auto) from_c(T &&handle) noexcept
    {
        using Cpp = CppType<T>;
        if constexpr (OpaqueHandle<Cpp>)
        {
            using Result = std::conditional_t<std::is_const_v<T>, const Cpp *, Cpp *>;
            return *reinterpret_cast<Result>(std::addressof(std::forward<T>(handle)));
        }
        else
        {
            return std::bit_cast<Cpp>(std::forward<T>(handle));
        }
    }

    export template <typename>
    struct CAliasableTraits;

    export template <typename T>
    concept CAlisable =
        requires { typename CAliasableTraits<std::remove_cvref_t<T>>::CType; } &&
        CHandle<typename CAliasableTraits<std::remove_cvref_t<T>>::CType> &&
        std::same_as<std::remove_cvref_t<T>, CppType<typename CAliasableTraits<std::remove_cvref_t<T>>::CType>>;

    export template <CAlisable T>
    using CType = CAliasableTraits<std::remove_cvref_t<T>>::CType;

    export template <CAlisable T>
        requires(CompatibleCHandle<CType<T>>)
    [[nodiscard]] constexpr auto to_c(T *ptr) noexcept
    {
        using C = CType<T>;
        using Result = std::conditional_t<std::is_const_v<T>, const C *, C *>;
        return reinterpret_cast<Result>(ptr);
    }

    export template <CAlisable T>
        requires(CompatibleCHandle<CType<T>> && (!OpaqueHandle<T> || std::is_lvalue_reference_v<T>))
    [[nodiscard]] constexpr decltype(auto) to_c(T &&data) noexcept
    {
        using C = CType<T>;
        if constexpr (OpaqueHandle<C>)
        {
            using Result = std::conditional_t<std::is_const_v<T>, const C *, C *>;
            return *reinterpret_cast<Result>(std::addressof(std::forward<T>(data)));
        }
        else
        {
            return std::bit_cast<C>(std::forward<T>(data));
        }
    }
} // namespace retro
