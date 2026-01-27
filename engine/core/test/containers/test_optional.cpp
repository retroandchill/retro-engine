/**
 * @file test_optional.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <catch2/catch_test_macros.hpp>

import retro.core;
import std;

namespace
{
    struct Counter
    {
        static inline int32 copies = 0;
        static inline int32 moves = 0;

        int32 value = 0;

        Counter() = default;
        explicit Counter(int32 v) : value{v}
        {
        }
        Counter(const Counter &other) : value{other.value}
        {
            ++copies;
        }
        Counter(Counter &&other) noexcept : value{other.value}
        {
            other.value = -1;
            ++moves;
        }

        Counter &operator=(const Counter &other)
        {
            value = other.value;
            ++copies;
            return *this;
        }

        Counter &operator=(Counter &&other) noexcept
        {
            value = other.value;
            other.value = -1;
            ++moves;
            return *this;
        }
    };

    void reset_counter()
    {
        Counter::copies = 0;
        Counter::moves = 0;
    }
} // namespace

TEST_CASE("Optional basic construction and observers")
{
    SECTION("default and nullopt construction")
    {
        retro::Optional<int32> empty{};
        retro::Optional<int32> nullopt{std::nullopt};

        REQUIRE_FALSE(empty.has_value());
        REQUIRE_FALSE(nullopt.has_value());
        REQUIRE_FALSE(static_cast<bool>(empty));
        REQUIRE(empty == std::nullopt);
        REQUIRE(nullopt == std::nullopt);
    }

    SECTION("value construction and access")
    {
        retro::Optional value{5};
        REQUIRE(value.has_value());
        REQUIRE(*value == 5);
        REQUIRE(value.value() == 5);
    }

    SECTION("value throws when empty")
    {
        retro::Optional<int32> empty{};
        REQUIRE_THROWS_AS(empty.value(), std::bad_optional_access);
    }
}

TEST_CASE("Optional copy and move")
{
    SECTION("copy construction and assignment")
    {
        retro::Optional<std::string> source{std::in_place, "hello"};
        retro::Optional copy{source};
        retro::Optional<std::string> assigned{};
        assigned = source;

        REQUIRE(copy.has_value());
        REQUIRE(assigned.has_value());
        REQUIRE(*copy == "hello");
        REQUIRE(*assigned == "hello");
    }

    SECTION("move construction and assignment")
    {
        retro::Optional<std::string> source{std::in_place, "world"};
        retro::Optional moved{std::move(source)};
        retro::Optional<std::string> assigned{};
        assigned = std::move(moved);

        REQUIRE(assigned.has_value());
        REQUIRE(*assigned == "world");
    }
}

TEST_CASE("Optional modifiers and value_or")
{
    SECTION("reset clears value")
    {
        retro::Optional value{9};
        value.reset();
        REQUIRE_FALSE(value.has_value());
    }

    SECTION("emplace constructs in place")
    {
        retro::Optional<std::string> value{};
        value.emplace(3, 'a');
        REQUIRE(value.has_value());
        REQUIRE(*value == "aaa");
    }

    SECTION("swap exchanges values")
    {
        retro::Optional a{1};
        retro::Optional<int32> b{};

        a.swap(b);
        REQUIRE_FALSE(a.has_value());
        REQUIRE(b.has_value());
        REQUIRE(*b == 1);
    }

    SECTION("value_or uses stored value")
    {
        retro::Optional value{7};
        REQUIRE(value.value_or(3) == 7);
    }

    SECTION("value_or uses fallback on empty")
    {
        retro::Optional<int32> value{};
        REQUIRE(value.value_or(3) == 3);
    }

    SECTION("value_or on rvalue moves the value")
    {
        retro::Optional value{Counter{11}};
        reset_counter();
        Counter result = std::move(value).value_or(Counter{4});

        REQUIRE(result.value == 11);
        REQUIRE(Counter::moves >= 1);
    }
}

TEST_CASE("Optional iterators and range usage")
{
    static_assert(std::ranges::range<retro::Optional<int32>>);
    static_assert(std::ranges::view<retro::Optional<int32>>);
    static_assert(std::ranges::input_range<retro::Optional<int32>>);
    static_assert(std::ranges::forward_range<retro::Optional<int32>>);
    static_assert(std::ranges::bidirectional_range<retro::Optional<int32>>);
    static_assert(std::ranges::contiguous_range<retro::Optional<int32>>);
    static_assert(std::ranges::common_range<retro::Optional<int32>>);
    static_assert(std::ranges::viewable_range<retro::Optional<int32>>);
    static_assert(!std::ranges::borrowed_range<retro::Optional<int32>>);
    static_assert(std::ranges::random_access_range<retro::Optional<int32>>);
    static_assert(std::ranges::sized_range<retro::Optional<int32>>);

    static_assert(std::ranges::range<retro::Optional<int32 &>>);
    static_assert(std::ranges::view<retro::Optional<int32 &>>);
    static_assert(std::ranges::input_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::forward_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::bidirectional_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::contiguous_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::common_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::viewable_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::borrowed_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::random_access_range<retro::Optional<int32 &>>);
    static_assert(std::ranges::sized_range<retro::Optional<int32 &>>);

    SECTION("empty optional has zero length")
    {
        retro::Optional<int32> empty{};
        REQUIRE(empty.begin() == empty.end());
        REQUIRE(std::distance(empty.begin(), empty.end()) == 0);
    }

    SECTION("engaged optional iterates over one element")
    {
        retro::Optional<int32> value{3};
        int32 total = 0;
        for (int32 v : value)
        {
            total += v;
        }

        REQUIRE(total == 3);
        REQUIRE(std::distance(value.begin(), value.end()) == 1);
    }
}

TEST_CASE("Optional comparisons match standard ordering rules")
{
    retro::Optional<int32> empty{};
    retro::Optional one{1};
    retro::Optional two{2};

    REQUIRE(empty == std::nullopt);
    REQUIRE(one != std::nullopt);
    REQUIRE(empty < one);
    REQUIRE(one < two);
    REQUIRE(two > one);
    REQUIRE(one == 1);
    REQUIRE(1 == one);
    REQUIRE(one != 2);
    REQUIRE(one < 2);
    REQUIRE(2 > one);
    REQUIRE((one <=> two) == std::strong_ordering::less);
    REQUIRE((one <=> std::nullopt) == std::strong_ordering::greater);
}

TEST_CASE("Optional monadic operations")
{
    SECTION("and_then propagates emptiness and applies when engaged")
    {
        retro::Optional value{2};
        auto doubled = value.and_then([](int32 v) { return retro::Optional{v * 2}; });
        REQUIRE(doubled.has_value());
        REQUIRE(*doubled == 4);

        retro::Optional<int32> empty{};
        auto result = empty.and_then([](int32 v) { return retro::Optional{v + 1}; });
        REQUIRE_FALSE(result.has_value());
    }

    SECTION("transform maps the contained value")
    {
        retro::Optional value{3};
        auto tripled = value.transform([](int32 v) { return v * 3; });
        REQUIRE(tripled.has_value());
        REQUIRE(*tripled == 9);

        retro::Optional<int32> empty{};
        auto empty_result = empty.transform([](int32 v) { return v * 3; });
        REQUIRE_FALSE(empty_result.has_value());
    }

    SECTION("or_else provides a fallback")
    {
        retro::Optional value{4};
        auto same = value.or_else([] { return retro::Optional{10}; });
        REQUIRE(same.has_value());
        REQUIRE(*same == 4);

        retro::Optional<int32> empty{};
        auto fallback = empty.or_else([] { return retro::Optional{10}; });
        REQUIRE(fallback.has_value());
        REQUIRE(*fallback == 10);
    }
}

TEST_CASE("make_optional helpers")
{
    auto value = retro::make_optional(5);
    REQUIRE(value.has_value());
    REQUIRE(*value == 5);

    auto text = retro::make_optional<std::string>(3, 'b');
    REQUIRE(text.has_value());
    REQUIRE(*text == "bbb");
}

TEST_CASE("Optional reference overload")
{
    static_assert(!std::is_constructible_v<retro::Optional<int32 &>, int32 &&>);

    SECTION("engaged optional references the original object")
    {
        int32 value = 10;
        retro::Optional<int32 &> ref{value};

        REQUIRE(ref.has_value());
        REQUIRE(&*ref == &value);

        *ref = 25;
        REQUIRE(value == 25);
    }

    SECTION("empty reference optional throws on value()")
    {
        retro::Optional<int32 &> empty{};
        REQUIRE_FALSE(empty.has_value());
        REQUIRE_THROWS_AS(empty.value(), std::bad_optional_access);
    }

    SECTION("value_or returns a copy when empty")
    {
        retro::Optional<int32 &> empty{};
        REQUIRE(empty.value_or(7) == 7);
    }

    SECTION("emplace rebinds to a new reference")
    {
        int32 a = 1;
        int32 b = 2;
        retro::Optional<int32 &> ref{a};

        ref.emplace(b);
        REQUIRE(ref.has_value());
        REQUIRE(&*ref == &b);

        b = 5;
        REQUIRE(*ref == 5);
    }

    SECTION("reference iterators behave like a single element range")
    {
        int32 value = 3;
        retro::Optional<int32 &> ref{value};
        int32 total = 0;
        for (int32 v : ref)
        {
            total += v;
        }

        REQUIRE(total == 3);
    }

    SECTION("monadic operations work with references")
    {
        int32 value = 4;
        retro::Optional<int32 &> ref{value};
        auto updated = ref.and_then(
            [](int32 &v)
            {
                v += 2;
                return retro::Optional{v};
            });

        REQUIRE(updated.has_value());
        REQUIRE(*updated == 6);
        REQUIRE(value == 6);

        auto mapped = ref.transform([](const int32 &v) { return v * 2; });
        REQUIRE(mapped.has_value());
        REQUIRE(*mapped == 12);

        retro::Optional<int32 &> empty{};
        int32 fallback_value = 9;
        auto fallback = empty.or_else([&] { return retro::Optional<int32 &>{fallback_value}; });
        REQUIRE(fallback.has_value());
        REQUIRE(&*fallback == &fallback_value);
    }
}
