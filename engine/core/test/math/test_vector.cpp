/**
 * @file test_vector.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_floating_point.hpp>

import retro.core;

using retro::Vector2f;
using retro::Vector2i;
using retro::Vector3f;
using retro::Vector3i;
using retro::Vector4f;
using retro::Vector4i;

TEST_CASE("Vector2 basic construction and members")
{
    SECTION("default construction")
    {
        Vector2i v{};
        REQUIRE(v.x == 0);
        REQUIRE(v.y == 0);
    }

    SECTION("component-wise construction")
    {
        Vector2i v{1, 2};
        REQUIRE(v.x == 1);
        REQUIRE(v.y == 2);
    }

    SECTION("broadcast construction")
    {
        Vector2i v{Vector2i::ValueType{5}};
        Vector2i w{5};

        REQUIRE(w.x == 5);
        REQUIRE(w.y == 5);
    }
}

TEST_CASE("Vector3 basic construction and members")
{
    SECTION("component-wise construction")
    {
        Vector3i v{1, 2, 3};
        REQUIRE(v.x == 1);
        REQUIRE(v.y == 2);
        REQUIRE(v.z == 3);
    }

    SECTION("construct from Vector2 + z")
    {
        Vector2i v2{4, 5};
        Vector3i v3{v2, 6};

        REQUIRE(v3.x == 4);
        REQUIRE(v3.y == 5);
        REQUIRE(v3.z == 6);
    }

    SECTION("broadcast construction")
    {
        Vector3i v{7};
        REQUIRE(v.x == 7);
        REQUIRE(v.y == 7);
        REQUIRE(v.z == 7);
    }
}

TEST_CASE("Vector4 basic construction and members")
{
    SECTION("default construction")
    {
        Vector4i v{};
        REQUIRE(v.x == 0);
        REQUIRE(v.y == 0);
        REQUIRE(v.z == 0);
        REQUIRE(v.w == 0);
    }

    SECTION("component-wise construction")
    {
        Vector4i v{1, 2, 3, 4};
        REQUIRE(v.x == 1);
        REQUIRE(v.y == 2);
        REQUIRE(v.z == 3);
        REQUIRE(v.w == 4);
    }

    SECTION("broadcast construction")
    {
        Vector4i v{9};
        REQUIRE(v.x == 9);
        REQUIRE(v.y == 9);
        REQUIRE(v.z == 9);
        REQUIRE(v.w == 9);
    }
}

TEST_CASE("Vector2 arithmetic operators")
{
    Vector2i a{1, 2};
    Vector2i b{3, 4};

    SECTION("operator+")
    {
        auto c = a + b;
        REQUIRE(c.x == 4);
        REQUIRE(c.y == 6);
    }

    SECTION("operator-")
    {
        auto c = b - a;
        REQUIRE(c.x == 2);
        REQUIRE(c.y == 2);
    }

    SECTION("operator* (scalar)")
    {
        auto c = a * 2;
        REQUIRE(c.x == 2);
        REQUIRE(c.y == 4);
    }

    SECTION("operator/ (scalar)")
    {
        Vector2f x{4.0f, 8.0f};
        auto c = x / 2.0f;
        REQUIRE_THAT(c.x, Catch::Matchers::WithinAbs(2.0f, 0.001));
        REQUIRE_THAT(c.y, Catch::Matchers::WithinAbs(4.0f, 0.001));
    }

    SECTION("operator+=")
    {
        Vector2i x{1, 2};
        Vector2i y{3, 4};
        x += y;
        REQUIRE(x.x == 4);
        REQUIRE(x.y == 6);
    }

    SECTION("operator-=")
    {
        Vector2i x{5, 7};
        Vector2i y{2, 3};
        x -= y;
        REQUIRE(x.x == 3);
        REQUIRE(x.y == 4);
    }

    SECTION("operator*=")
    {
        Vector2i x{2, 3};
        x *= 3;
        REQUIRE(x.x == 6);
        REQUIRE(x.y == 9);
    }

    SECTION("operator/=")
    {
        Vector2f x{6.0f, 9.0f};
        x /= 3.0f;
        REQUIRE_THAT(x.x, Catch::Matchers::WithinAbs(2.0f, 0.001));
        REQUIRE_THAT(x.y, Catch::Matchers::WithinAbs(3.0f, 0.001));
    }

    SECTION("operator==")
    {
        Vector2i x{1, 2};
        Vector2i y{1, 2};
        Vector2i z{2, 3};

        REQUIRE(x == y);
        REQUIRE_FALSE(x == z);
    }
}

TEST_CASE("Vector3 arithmetic operators")
{
    Vector3i a{1, 2, 3};
    Vector3i b{4, 5, 6};

    SECTION("operator+")
    {
        auto c = a + b;
        REQUIRE(c.x == 5);
        REQUIRE(c.y == 7);
        REQUIRE(c.z == 9);
    }

    SECTION("operator-")
    {
        auto c = b - a;
        REQUIRE(c.x == 3);
        REQUIRE(c.y == 3);
        REQUIRE(c.z == 3);
    }

    SECTION("operator* (scalar)")
    {
        auto c = a * 2;
        REQUIRE(c.x == 2);
        REQUIRE(c.y == 4);
        REQUIRE(c.z == 6);
    }

    SECTION("operator/ (scalar)")
    {
        Vector3f x{6.0f, 8.0f, 10.0f};
        auto c = x / 2.0f;
        REQUIRE_THAT(c.x, Catch::Matchers::WithinAbs(3.0f, 0.001));
        REQUIRE_THAT(c.y, Catch::Matchers::WithinAbs(4.0f, 0.001));
        REQUIRE_THAT(c.z, Catch::Matchers::WithinAbs(5.0f, 0.001));
    }

    SECTION("operator+=")
    {
        Vector3i x{1, 2, 3};
        Vector3i y{4, 5, 6};
        x += y;
        REQUIRE(x.x == 5);
        REQUIRE(x.y == 7);
        REQUIRE(x.z == 9);
    }

    SECTION("operator-=")
    {
        Vector3i x{5, 7, 9};
        Vector3i y{1, 2, 3};
        x -= y;
        REQUIRE(x.x == 4);
        REQUIRE(x.y == 5);
        REQUIRE(x.z == 6);
    }

    SECTION("operator*=")
    {
        Vector3i x{1, 2, 3};
        x *= 4;
        REQUIRE(x.x == 4);
        REQUIRE(x.y == 8);
        REQUIRE(x.z == 12);
    }

    SECTION("operator/=")
    {
        Vector3f x{4.0f, 6.0f, 8.0f};
        x /= 2.0f;

        REQUIRE_THAT(x.x, Catch::Matchers::WithinAbs(2.0f, 0.001));
        REQUIRE_THAT(x.y, Catch::Matchers::WithinAbs(3.0f, 0.001));
        REQUIRE_THAT(x.z, Catch::Matchers::WithinAbs(4.0f, 0.001));
    }

    SECTION("operator==")
    {
        Vector3i x{1, 2, 3};
        Vector3i y{1, 2, 3};
        Vector3i z{0, 0, 0};

        REQUIRE(x == y);
        REQUIRE_FALSE(x == z);
    }
}

TEST_CASE("Vector4 arithmetic operators")
{
    Vector4i a{1, 2, 3, 4};
    Vector4i b{5, 6, 7, 8};

    SECTION("operator+")
    {
        auto c = a + b;
        REQUIRE(c.x == 6);
        REQUIRE(c.y == 8);
        REQUIRE(c.z == 10);
        REQUIRE(c.w == 12);
    }

    SECTION("operator-")
    {
        auto c = b - a;
        REQUIRE(c.x == 4);
        REQUIRE(c.y == 4);
        REQUIRE(c.z == 4);
        REQUIRE(c.w == 4);
    }

    SECTION("operator* (scalar)")
    {
        auto c = a * 2;
        REQUIRE(c.x == 2);
        REQUIRE(c.y == 4);
        REQUIRE(c.z == 6);
        REQUIRE(c.w == 8);
    }

    SECTION("operator/ (scalar)")
    {
        Vector4f x{8.0f, 10.0f, 12.0f, 14.0f};
        auto c = x / 2.0f;
        REQUIRE_THAT(c.x, Catch::Matchers::WithinAbs(4.0f, 0.001));
        REQUIRE_THAT(c.y, Catch::Matchers::WithinAbs(5.0f, 0.001));
        REQUIRE_THAT(c.z, Catch::Matchers::WithinAbs(6.0f, 0.001));
        REQUIRE_THAT(c.w, Catch::Matchers::WithinAbs(7.0f, 0.001));
    }

    SECTION("operator+=")
    {
        Vector4i x{1, 2, 3, 4};
        Vector4i y{5, 6, 7, 8};
        x += y;
        REQUIRE(x.x == 6);
        REQUIRE(x.y == 8);
        REQUIRE(x.z == 10);
        REQUIRE(x.w == 12);
    }

    SECTION("operator-=")
    {
        Vector4i x{10, 11, 12, 13};
        Vector4i y{1, 2, 3, 4};
        x -= y;
        REQUIRE(x.x == 9);
        REQUIRE(x.y == 9);
        REQUIRE(x.z == 9);
        REQUIRE(x.w == 9);
    }

    SECTION("operator*=")
    {
        Vector4i x{1, 2, 3, 4};
        x *= 3;
        REQUIRE(x.x == 3);
        REQUIRE(x.y == 6);
        REQUIRE(x.z == 9);
        REQUIRE(x.w == 12);
    }

    SECTION("operator/=")
    {
        Vector4f x{4.0f, 6.0f, 8.0f, 10.0f};
        x /= 2.0f;
        REQUIRE_THAT(x.x, Catch::Matchers::WithinAbs(2.0f, 0.001));
        REQUIRE_THAT(x.y, Catch::Matchers::WithinAbs(3.0f, 0.001));
        REQUIRE_THAT(x.z, Catch::Matchers::WithinAbs(4.0f, 0.001));
        REQUIRE_THAT(x.w, Catch::Matchers::WithinAbs(5.0f, 0.001));
    }

    SECTION("operator==")
    {
        Vector4i x{1, 2, 3, 4};
        Vector4i y{1, 2, 3, 4};
        Vector4i z{0, 0, 0, 0};

        REQUIRE(x == y);
        REQUIRE_FALSE(x == z);
    }
}

TEST_CASE("Vector constexpr usage")
{
    SECTION("Vector2i constexpr arithmetic")
    {
        constexpr retro::Vector2i a{1, 2};
        constexpr retro::Vector2i b{3, 4};
        constexpr auto c = a + b;
        static_assert(c.x == 4);
        static_assert(c.y == 6);
    }

    SECTION("Vector3f constexpr arithmetic")
    {
        constexpr retro::Vector3f a{1.0f, 2.0f, 3.0f};
        constexpr auto b = a * 2.0f;
        static_assert(b.x == 2.0f);
        static_assert(b.y == 4.0f);
        static_assert(b.z == 6.0f);
    }

    SECTION("Vector4i constexpr arithmetic")
    {
        constexpr retro::Vector4i a{1, 2, 3, 4};
        constexpr auto b = a + retro::Vector4i{4, 3, 2, 1};
        static_assert(b.x == 5);
        static_assert(b.y == 5);
        static_assert(b.z == 5);
        static_assert(b.w == 5);
    }
}

TEST_CASE("Vector structured bindings")
{
    SECTION("Vector2")
    {
        Vector2i v{10, 20};
        auto [x, y] = v;

        REQUIRE(x == 10);
        REQUIRE(y == 20);
    }

    SECTION("Vector3")
    {
        Vector3f v{1.0f, 2.5f, 3.75f};
        auto [x, y, z] = v;

        REQUIRE_THAT(x, Catch::Matchers::WithinAbs(1.0f, 0.001));
        REQUIRE_THAT(y, Catch::Matchers::WithinAbs(2.5f, 0.001));
        REQUIRE_THAT(z, Catch::Matchers::WithinAbs(3.75f, 0.001));
    }

    SECTION("Vector4")
    {
        Vector4i v{1, 2, 3, 4};
        auto [x, y, z, w] = v;

        REQUIRE(x == 1);
        REQUIRE(y == 2);
        REQUIRE(z == 3);
        REQUIRE(w == 4);
    }
}
