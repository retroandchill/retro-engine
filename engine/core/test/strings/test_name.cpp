/**
 * @file test_name.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "gtest_helpers.hpp"

#include <gtest/gtest.h>

using retro::FindType;
using retro::Name;
using retro::NAME_NO_NUMBER;

TEST(Name, DefaultConstructionYieldsNoneAndValid)
{
    constexpr Name n;

    EXPECT_TRUE(n.is_none());
    EXPECT_TRUE(n.is_valid());
    EXPECT_EQ(n.comparison_index(), 0u);
    EXPECT_EQ(n.display_index(), 0u);
    EXPECT_EQ(n.number(), NAME_NO_NUMBER);
}

TEST(Name, SameLogicalValueSharesComparisonIndex)
{
    // Case-insensitive comparison index, but equality uses comparison_index + number.
    Name upper{std::u16string{u"Player"}};
    Name lower{std::u16string{u"player"}};

    ASSERT_TRUE(upper.is_valid());
    ASSERT_TRUE(lower.is_valid());

    // Same logical name => same comparison index
    EXPECT_EQ(upper.comparison_index(), lower.comparison_index());

    // Display strings are case-sensitive; they may differ
    EXPECT_NE(upper.display_index(), 0u);
    EXPECT_NE(lower.display_index(), 0u);
    EXPECT_NE(upper.display_index(), lower.display_index());

    // Equality operator only cares about comparison_index + number
    EXPECT_TRUE(upper == lower);
}

TEST(Name, NumericSuffixParsesNumberAndKeepsBaseString)
{
    Name n{std::u16string{u"Enemy_42"}};

    ASSERT_TRUE(n.is_valid());
    EXPECT_FALSE(n.is_none());

    // Number gets parsed out
    EXPECT_EQ(n.number(), 42);

    // Stored base name should be with the _42 suffix
    const std::string base = n.to_string();
    EXPECT_EQ(base, (std::string{"Enemy_42"}));

    // Comparison is still against the base logical name
    EXPECT_TRUE(n == std::string_view{"Enemy_42"});
}

TEST(Name, IgnoresInvalidOrMalformedNumericSuffixes)
{
    // Leading zero after underscore should be rejected as a number
    Name with_leading_zero{std::u16string{u"Foo_01"}};

    ASSERT_TRUE(with_leading_zero.is_valid());
    EXPECT_EQ(with_leading_zero.number(), NAME_NO_NUMBER);
    EXPECT_EQ(with_leading_zero.to_string(), (std::string{"Foo_01"}));

    // No underscore before digits -> treated as part of the name
    Name no_underscore{std::string{"Bar99"}};
    ASSERT_TRUE(no_underscore.is_valid());
    EXPECT_EQ(no_underscore.number(), NAME_NO_NUMBER);
    EXPECT_EQ(no_underscore.to_string(), (std::string{"Bar99"}));
}

TEST(Name, FindTypeFindDoesNotCreateNewEntries)
{
    // First ensure there is one known entry
    Name existing{std::u16string{u"Knight"}};
    ASSERT_TRUE(existing.is_valid());

    const auto existing_comparison = existing.comparison_index();
    const auto existing_display = existing.display_index();

    // Lookup again using FindType::Find -> should find the same indices
    Name found_existing{std::u16string{u"Knight"}, FindType::find};
    ASSERT_TRUE(found_existing.is_valid());
    EXPECT_FALSE(found_existing.is_none());
    EXPECT_EQ(found_existing.comparison_index(), existing_comparison);
    EXPECT_EQ(found_existing.display_index(), existing_display);

    // Lookup unknown name with FindType::Find -> should yield a "none" name
    Name not_created{std::u16string{u"UnknownNameThatDoesNotExist"}, FindType::find};
    EXPECT_TRUE(not_created.is_none());
    EXPECT_TRUE(not_created.is_valid());
    EXPECT_EQ(not_created.comparison_index(), 0u);
    EXPECT_EQ(not_created.display_index(), 0u);
}

TEST(Name, EqualsComparisonIsCaseInsensitive)
{
    Name n{std::u16string{u"Boss"}};

    ASSERT_TRUE(n.is_valid());

    // Comparison against std::u16string_view is case-insensitive
    EXPECT_TRUE(n == std::u16string_view{u"boss"});
    EXPECT_TRUE(n == std::u16string_view{u"BOSS"});
    EXPECT_FALSE(n == std::u16string_view{u"miniboss"});
}

TEST(Name, NoneCreatesANoneName)
{
    const Name none = Name::none();

    EXPECT_TRUE(none.is_none());
    EXPECT_TRUE(none.is_valid());
    EXPECT_EQ(none.comparison_index(), 0u);
    EXPECT_EQ(none.display_index(), 0u);
    EXPECT_EQ(none.number(), NAME_NO_NUMBER);

    // String representation for an invalid display index should be "None"
    const std::string display = none.to_string();
    EXPECT_EQ(display, (std::string{"None"}));
}
