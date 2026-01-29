/**
 * @file concepts.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:concepts;

import std;
import :defines;

namespace retro
{
    /**
     * Mixin class to define a simple type that is not able to be copied or moved.
     */
    export class NonCopyable
    {
      protected:
        /**
         * Default constructor, marked as protected to prevent direct instantiation.
         */
        NonCopyable() = default;

        /**
         * Default no-op destructor, marked protected to discourage managing a pointer to this type.
         */
        ~NonCopyable() = default;

      public:
        NonCopyable(const NonCopyable &) = delete;
        NonCopyable(NonCopyable &&) = delete;

        NonCopyable &operator=(const NonCopyable &) = delete;
        NonCopyable &operator=(NonCopyable &&) = delete;
    };

    export template <typename T, typename U>
    using ForwardLikeType =
        std::conditional_t<std::is_const_v<std::remove_reference_t<T>>,
                           std::conditional_t<std::is_lvalue_reference_v<T>,
                                              std::add_const_t<U>,
                                              std::add_const_t<std::remove_reference_t<U> &&>>,
                           std::conditional_t<std::is_lvalue_reference_v<T>, U, std::remove_reference_t<U> &&>>;

    template <typename To, typename From>
    concept ReferenceConvertsFromTemporary =
        std::is_reference_v<To> && ((!std::is_reference_v<From> &&
                                     std::is_convertible_v<std::remove_cvref_t<From> *, std::remove_cvref_t<To> *>) ||
                                    (std::is_lvalue_reference_v<To> && std::is_const_v<std::remove_reference_t<To>> &&
                                     std::is_convertible_v<From, const std::remove_cvref_t<To> &&> &&
                                     !std::is_convertible_v<From, std::remove_cvref_t<To> &>));

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

    template <typename>
    constexpr bool IS_VARIANT_IMPLEMENTATION = false;

    template <typename... T>
    constexpr bool IS_VARIANT_IMPLEMENTATION<std::variant<T...>> = true;

    export template <typename Variant>
    concept VariantSpecialization = IS_VARIANT_IMPLEMENTATION<std::remove_cvref_t<Variant>>;

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

    export template <typename T>
    using PointerElementT = PointerElement<std::remove_cvref_t<T>>::Type;

    template <WeakBindable T>
    struct WeakBindableElement
    {
      private:
        using Raw = std::remove_cvref_t<T>;

      public:
        using Type = std::conditional_t<SharedPtrLike<Raw> || WeakPtrLike<Raw>,
                                        PointerElementT<Raw>,
                                        PointerElementT<decltype(std::declval<Raw &>().weak_from_this())>>;
    };

    export template <WeakBindable T>
    using WeakBindableElementT = WeakBindableElement<T>::Type;

    export template <typename T>
    concept DecayCopyable = std::constructible_from<std::decay_t<T>, T>;

    export template <typename T, typename U>
    concept EqualityComparableWith = requires(const T &t, const U &u) {
        {
            t == u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept InequalityComparableWith = requires(const T &t, const U &u) {
        {
            t != u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept LessThanComparableWith = requires(const T &t, const U &u) {
        {
            t < u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept GreaterThanComparableWith = requires(const T &t, const U &u) {
        {
            t > u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept LessThanOrEqualComparableWith = requires(const T &t, const U &u) {
        {
            t <= u
        } -> std::convertible_to<bool>;
    };

    export template <typename T, typename U>
    concept GreaterThanOrEqualComparableWith = requires(const T &t, const U &u) {
        {
            t >= u
        } -> std::convertible_to<bool>;
    };

    export template <typename T>
    concept Hashable = requires(T a) {
        {
            std::hash<std::remove_const_t<T>>{}(a)
        } -> std::convertible_to<usize>;
    };

    export template <typename Container, typename Reference>
    concept ContainerAppendable = requires(Container &c, Reference &&ref) {
        requires requires { c.emplace_back(std::forward<Reference>(ref)); } ||
                     requires { c.push_back(std::forward<Reference>(ref)); } ||
                     requires { c.emplace(c.end(), std::forward<Reference>(ref)); } ||
                     requires { c.insert(c.end(), std::forward<Reference>(ref)); };
    };

    export template <typename R, typename T>
    concept ContainerCompatibleRange =
        std::ranges::input_range<R> && std::convertible_to<std::ranges::range_reference_t<R>, T>;
} // namespace retro
