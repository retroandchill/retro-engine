/**
 * @file variant.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.type_traits.variant;

import std;

namespace retro
{
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
    constexpr bool is_variant_implementation = false;

    template <typename... T>
    constexpr bool is_variant_implementation<std::variant<T...>> = true;

    export template <typename Variant>
    concept VariantSpecialization = is_variant_implementation<std::remove_cvref_t<Variant>>;

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
} // namespace retro
