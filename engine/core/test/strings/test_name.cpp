/**
 * @file test_name.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "test_helpers.h"

#include <catch2/catch_test_macros.hpp>

import retro.core;

using retro::FindType;
using retro::Name;
using retro::NAME_NO_NUMBER;

TEST_CASE("Name default construction yields none and invalid", "[name]")
{
    constexpr Name n;

    REQUIRE(n.is_none());
    REQUIRE_FALSE(n.is_valid());
    REQUIRE(n.comparison_index() == 0u);
    REQUIRE(n.display_index() == 0u);
    REQUIRE(n.number() == NAME_NO_NUMBER);
}

TEST_CASE("Name created from same logical value shares comparison index", "[name]")
{
    // Case-insensitive comparison index, but equality uses comparison_index + number.
    Name upper{std::u16string{u"Player"}};
    Name lower{std::u16string{u"player"}};

    REQUIRE(upper.is_valid());
    REQUIRE(lower.is_valid());

    // Same logical name => same comparison index
    REQUIRE(upper.comparison_index() == lower.comparison_index());

    // Display strings are case-sensitive; they may differ
    REQUIRE(upper.display_index() != 0u);
    REQUIRE(lower.display_index() != 0u);
    REQUIRE(upper.display_index() != lower.display_index());

    // Equality operator only cares about comparison_index + number
    REQUIRE(upper == lower);
}

TEST_CASE("Name with numeric suffix parses number and strips it from base", "[name]")
{
    Name n{std::u16string{u"Enemy_42"}};

    REQUIRE(n.is_valid());
    REQUIRE_FALSE(n.is_none());

    // Number gets parsed out
    REQUIRE(n.number() == 43);

    // Stored base name should be with the _42 suffix
    const std::u16string base = n.to_string();
    REQUIRE(base == std::u16string{u"Enemy_42"});

    // Comparison is still against the base logical name
    REQUIRE(n == std::u16string_view{u"Enemy_42"});
}

TEST_CASE("Name ignores invalid or malformed numeric suffixes", "[name]")
{
    // Leading zero after underscore should be rejected as a number
    Name with_leading_zero{std::u16string{u"Foo_01"}};

    REQUIRE(with_leading_zero.is_valid());
    REQUIRE(with_leading_zero.number() == NAME_NO_NUMBER);
    REQUIRE(with_leading_zero.to_string() == std::u16string{u"Foo_01"});

    // No underscore before digits -> treated as part of the name
    Name no_underscore{std::u16string{u"Bar99"}};
    REQUIRE(no_underscore.is_valid());
    REQUIRE(no_underscore.number() == NAME_NO_NUMBER);
    REQUIRE(no_underscore.to_string() == std::u16string{u"Bar99"});
}

TEST_CASE("FindType::Find does not create new entries", "[name]")
{
    // First ensure there is one known entry
    Name existing{std::u16string{u"Knight"}};
    REQUIRE(existing.is_valid());
    // ReSharper disable once CppDFAUnreadVariable
    // ReSharper disable once CppDFAUnusedValue
    const auto existing_comparison = existing.comparison_index();
    // ReSharper disable once CppDFAUnreadVariable
    // ReSharper disable once CppDFAUnusedValue
    const auto existing_display = existing.display_index();

    // Lookup again using FindType::Find -> should find the same indices
    Name found_existing{std::u16string{u"Knight"}, FindType::Find};
    REQUIRE(found_existing.is_valid());
    REQUIRE_FALSE(found_existing.is_none());
    REQUIRE(found_existing.comparison_index() == existing_comparison);
    REQUIRE(found_existing.display_index() == existing_display);

    // Lookup unknown name with FindType::Find -> should yield a "none" name
    Name not_created{std::u16string{u"UnknownNameThatDoesNotExist"}, FindType::Find};
    REQUIRE(not_created.is_none());
    REQUIRE_FALSE(not_created.is_valid());
    REQUIRE(not_created.comparison_index() == 0u);
    REQUIRE(not_created.display_index() == 0u);
}

TEST_CASE("Name equals comparison uses case-insensitive semantic", "[name]")
{
    Name n{std::u16string{u"Boss"}};

    REQUIRE(n.is_valid());

    // Comparison against std::u16string_view is case-insensitive
    REQUIRE(n == std::u16string_view{u"boss"});
    REQUIRE(n == std::u16string_view{u"BOSS"});
    REQUIRE_FALSE(n == std::u16string_view{u"miniboss"});
}

TEST_CASE("Name none() creates a none name", "[name]")
{
    const Name none = Name::none();

    REQUIRE(none.is_none());
    REQUIRE_FALSE(none.is_valid());
    REQUIRE(none.comparison_index() == 0u);
    REQUIRE(none.display_index() == 0u);
    REQUIRE(none.number() == NAME_NO_NUMBER);

    // String representation for an invalid display index should be "None"
    const std::u16string display = none.to_string();
    REQUIRE(display == std::u16string{u"None"});
}
