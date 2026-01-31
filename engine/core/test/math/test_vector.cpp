/**
 * @file test_vector.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.core;

using retro::Vector2f;
using retro::Vector2i;
using retro::Vector3f;
using retro::Vector3i;
using retro::Vector4f;
using retro::Vector4i;

// -------------------- Vector2 basic construction and members --------------------

TEST(Vector2, DefaultConstruction)
{
    Vector2i v{};
    EXPECT_EQ(v.x, 0);
    EXPECT_EQ(v.y, 0);
}

TEST(Vector2, ComponentWiseConstruction)
{
    Vector2i v{1, 2};
    EXPECT_EQ(v.x, 1);
    EXPECT_EQ(v.y, 2);
}

TEST(Vector2, BroadcastConstruction)
{
    Vector2i v{Vector2i::ValueType{5}};
    Vector2i w{5};

    EXPECT_EQ(w.x, 5);
    EXPECT_EQ(w.y, 5);
    (void)v; // keep `v` if you want parity with the original structure
}

// -------------------- Vector3 basic construction and members --------------------

TEST(Vector3, ComponentWiseConstruction)
{
    Vector3i v{1, 2, 3};
    EXPECT_EQ(v.x, 1);
    EXPECT_EQ(v.y, 2);
    EXPECT_EQ(v.z, 3);
}

TEST(Vector3, ConstructFromVector2PlusZ)
{
    Vector2i v2{4, 5};
    Vector3i v3{v2, 6};

    EXPECT_EQ(v3.x, 4);
    EXPECT_EQ(v3.y, 5);
    EXPECT_EQ(v3.z, 6);
}

TEST(Vector3, BroadcastConstruction)
{
    Vector3i v{7};
    EXPECT_EQ(v.x, 7);
    EXPECT_EQ(v.y, 7);
    EXPECT_EQ(v.z, 7);
}

// -------------------- Vector4 basic construction and members --------------------

TEST(Vector4, DefaultConstruction)
{
    Vector4i v{};
    EXPECT_EQ(v.x, 0);
    EXPECT_EQ(v.y, 0);
    EXPECT_EQ(v.z, 0);
    EXPECT_EQ(v.w, 0);
}

TEST(Vector4, ComponentWiseConstruction)
{
    Vector4i v{1, 2, 3, 4};
    EXPECT_EQ(v.x, 1);
    EXPECT_EQ(v.y, 2);
    EXPECT_EQ(v.z, 3);
    EXPECT_EQ(v.w, 4);
}

TEST(Vector4, BroadcastConstruction)
{
    Vector4i v{9};
    EXPECT_EQ(v.x, 9);
    EXPECT_EQ(v.y, 9);
    EXPECT_EQ(v.z, 9);
    EXPECT_EQ(v.w, 9);
}

// -------------------- Vector2 arithmetic operators --------------------

TEST(Vector2, OperatorPlus)
{
    Vector2i a{1, 2};
    Vector2i b{3, 4};
    auto c = a + b;

    EXPECT_EQ(c.x, 4);
    EXPECT_EQ(c.y, 6);
}

TEST(Vector2, OperatorMinus)
{
    Vector2i a{1, 2};
    Vector2i b{3, 4};
    auto c = b - a;

    EXPECT_EQ(c.x, 2);
    EXPECT_EQ(c.y, 2);
}

TEST(Vector2, OperatorMultiplyScalar)
{
    Vector2i a{1, 2};
    auto c = a * 2;

    EXPECT_EQ(c.x, 2);
    EXPECT_EQ(c.y, 4);
}

TEST(Vector2, OperatorDivideScalar)
{
    Vector2f x{4.0f, 8.0f};
    auto c = x / 2.0f;

    EXPECT_NEAR(c.x, 2.0f, 0.001f);
    EXPECT_NEAR(c.y, 4.0f, 0.001f);
}

TEST(Vector2, OperatorPlusEquals)
{
    Vector2i x{1, 2};
    Vector2i y{3, 4};
    x += y;

    EXPECT_EQ(x.x, 4);
    EXPECT_EQ(x.y, 6);
}

TEST(Vector2, OperatorMinusEquals)
{
    Vector2i x{5, 7};
    Vector2i y{2, 3};
    x -= y;

    EXPECT_EQ(x.x, 3);
    EXPECT_EQ(x.y, 4);
}

TEST(Vector2, OperatorMultiplyEquals)
{
    Vector2i x{2, 3};
    x *= 3;

    EXPECT_EQ(x.x, 6);
    EXPECT_EQ(x.y, 9);
}

TEST(Vector2, OperatorDivideEquals)
{
    Vector2f x{6.0f, 9.0f};
    x /= 3.0f;

    EXPECT_NEAR(x.x, 2.0f, 0.001f);
    EXPECT_NEAR(x.y, 3.0f, 0.001f);
}

TEST(Vector2, OperatorEqualsEquals)
{
    Vector2i x{1, 2};
    Vector2i y{1, 2};
    Vector2i z{2, 3};

    EXPECT_TRUE(x == y);
    EXPECT_FALSE(x == z);
}

// -------------------- Vector3 arithmetic operators --------------------

TEST(Vector3, OperatorPlus)
{
    Vector3i a{1, 2, 3};
    Vector3i b{4, 5, 6};
    auto c = a + b;

    EXPECT_EQ(c.x, 5);
    EXPECT_EQ(c.y, 7);
    EXPECT_EQ(c.z, 9);
}

TEST(Vector3, OperatorMinus)
{
    Vector3i a{1, 2, 3};
    Vector3i b{4, 5, 6};
    auto c = b - a;

    EXPECT_EQ(c.x, 3);
    EXPECT_EQ(c.y, 3);
    EXPECT_EQ(c.z, 3);
}

TEST(Vector3, OperatorMultiplyScalar)
{
    Vector3i a{1, 2, 3};
    auto c = a * 2;

    EXPECT_EQ(c.x, 2);
    EXPECT_EQ(c.y, 4);
    EXPECT_EQ(c.z, 6);
}

TEST(Vector3, OperatorDivideScalar)
{
    Vector3f x{6.0f, 8.0f, 10.0f};
    auto c = x / 2.0f;

    EXPECT_NEAR(c.x, 3.0f, 0.001f);
    EXPECT_NEAR(c.y, 4.0f, 0.001f);
    EXPECT_NEAR(c.z, 5.0f, 0.001f);
}

TEST(Vector3, OperatorPlusEquals)
{
    Vector3i x{1, 2, 3};
    Vector3i y{4, 5, 6};
    x += y;

    EXPECT_EQ(x.x, 5);
    EXPECT_EQ(x.y, 7);
    EXPECT_EQ(x.z, 9);
}

TEST(Vector3, OperatorMinusEquals)
{
    Vector3i x{5, 7, 9};
    Vector3i y{1, 2, 3};
    x -= y;

    EXPECT_EQ(x.x, 4);
    EXPECT_EQ(x.y, 5);
    EXPECT_EQ(x.z, 6);
}

TEST(Vector3, OperatorMultiplyEquals)
{
    Vector3i x{1, 2, 3};
    x *= 4;

    EXPECT_EQ(x.x, 4);
    EXPECT_EQ(x.y, 8);
    EXPECT_EQ(x.z, 12);
}

TEST(Vector3, OperatorDivideEquals)
{
    Vector3f x{4.0f, 6.0f, 8.0f};
    x /= 2.0f;

    EXPECT_NEAR(x.x, 2.0f, 0.001f);
    EXPECT_NEAR(x.y, 3.0f, 0.001f);
    EXPECT_NEAR(x.z, 4.0f, 0.001f);
}

TEST(Vector3, OperatorEqualsEquals)
{
    Vector3i x{1, 2, 3};
    Vector3i y{1, 2, 3};
    Vector3i z{0, 0, 0};

    EXPECT_TRUE(x == y);
    EXPECT_FALSE(x == z);
}

// -------------------- Vector4 arithmetic operators --------------------

TEST(Vector4, OperatorPlus)
{
    Vector4i a{1, 2, 3, 4};
    Vector4i b{5, 6, 7, 8};
    auto c = a + b;

    EXPECT_EQ(c.x, 6);
    EXPECT_EQ(c.y, 8);
    EXPECT_EQ(c.z, 10);
    EXPECT_EQ(c.w, 12);
}

TEST(Vector4, OperatorMinus)
{
    Vector4i a{1, 2, 3, 4};
    Vector4i b{5, 6, 7, 8};
    auto c = b - a;

    EXPECT_EQ(c.x, 4);
    EXPECT_EQ(c.y, 4);
    EXPECT_EQ(c.z, 4);
    EXPECT_EQ(c.w, 4);
}

TEST(Vector4, OperatorMultiplyScalar)
{
    Vector4i a{1, 2, 3, 4};
    auto c = a * 2;

    EXPECT_EQ(c.x, 2);
    EXPECT_EQ(c.y, 4);
    EXPECT_EQ(c.z, 6);
    EXPECT_EQ(c.w, 8);
}

TEST(Vector4, OperatorDivideScalar)
{
    Vector4f x{8.0f, 10.0f, 12.0f, 14.0f};
    auto c = x / 2.0f;

    EXPECT_NEAR(c.x, 4.0f, 0.001f);
    EXPECT_NEAR(c.y, 5.0f, 0.001f);
    EXPECT_NEAR(c.z, 6.0f, 0.001f);
    EXPECT_NEAR(c.w, 7.0f, 0.001f);
}

TEST(Vector4, OperatorPlusEquals)
{
    Vector4i x{1, 2, 3, 4};
    Vector4i y{5, 6, 7, 8};
    x += y;

    EXPECT_EQ(x.x, 6);
    EXPECT_EQ(x.y, 8);
    EXPECT_EQ(x.z, 10);
    EXPECT_EQ(x.w, 12);
}

TEST(Vector4, OperatorMinusEquals)
{
    Vector4i x{10, 11, 12, 13};
    Vector4i y{1, 2, 3, 4};
    x -= y;

    EXPECT_EQ(x.x, 9);
    EXPECT_EQ(x.y, 9);
    EXPECT_EQ(x.z, 9);
    EXPECT_EQ(x.w, 9);
}

TEST(Vector4, OperatorMultiplyEquals)
{
    Vector4i x{1, 2, 3, 4};
    x *= 3;

    EXPECT_EQ(x.x, 3);
    EXPECT_EQ(x.y, 6);
    EXPECT_EQ(x.z, 9);
    EXPECT_EQ(x.w, 12);
}

TEST(Vector4, OperatorDivideEquals)
{
    Vector4f x{4.0f, 6.0f, 8.0f, 10.0f};
    x /= 2.0f;

    EXPECT_NEAR(x.x, 2.0f, 0.001f);
    EXPECT_NEAR(x.y, 3.0f, 0.001f);
    EXPECT_NEAR(x.z, 4.0f, 0.001f);
    EXPECT_NEAR(x.w, 5.0f, 0.001f);
}

TEST(Vector4, OperatorEqualsEquals)
{
    Vector4i x{1, 2, 3, 4};
    Vector4i y{1, 2, 3, 4};
    Vector4i z{0, 0, 0, 0};

    EXPECT_TRUE(x == y);
    EXPECT_FALSE(x == z);
}

// -------------------- Vector constexpr usage --------------------

TEST(VectorConstexpr, Vector2iConstexprArithmetic)
{
    constexpr retro::Vector2i a{1, 2};
    constexpr retro::Vector2i b{3, 4};
    constexpr auto c = a + b;

    static_assert(c.x == 4);
    static_assert(c.y == 6);

    // Also assert at runtime so the test runner "sees" something meaningful:
    EXPECT_EQ(c.x, 4);
    EXPECT_EQ(c.y, 6);
}

TEST(VectorConstexpr, Vector3fConstexprArithmetic)
{
    constexpr retro::Vector3f a{1.0f, 2.0f, 3.0f};
    constexpr auto b = a * 2.0f;

    static_assert(b.x == 2.0f);
    static_assert(b.y == 4.0f);
    static_assert(b.z == 6.0f);

    EXPECT_FLOAT_EQ(b.x, 2.0f);
    EXPECT_FLOAT_EQ(b.y, 4.0f);
    EXPECT_FLOAT_EQ(b.z, 6.0f);
}

TEST(VectorConstexpr, Vector4iConstexprArithmetic)
{
    constexpr retro::Vector4i a{1, 2, 3, 4};
    constexpr auto b = a + retro::Vector4i{4, 3, 2, 1};

    static_assert(b.x == 5);
    static_assert(b.y == 5);
    static_assert(b.z == 5);
    static_assert(b.w == 5);

    EXPECT_EQ(b.x, 5);
    EXPECT_EQ(b.y, 5);
    EXPECT_EQ(b.z, 5);
    EXPECT_EQ(b.w, 5);
}

// -------------------- Vector structured bindings --------------------

TEST(VectorStructuredBindings, Vector2)
{
    Vector2i v{10, 20};
    auto [x, y] = v;

    EXPECT_EQ(x, 10);
    EXPECT_EQ(y, 20);
}

TEST(VectorStructuredBindings, Vector3)
{
    Vector3f v{1.0f, 2.5f, 3.75f};
    auto [x, y, z] = v;

    EXPECT_NEAR(x, 1.0f, 0.001f);
    EXPECT_NEAR(y, 2.5f, 0.001f);
    EXPECT_NEAR(z, 3.75f, 0.001f);
}

TEST(VectorStructuredBindings, Vector4)
{
    Vector4i v{1, 2, 3, 4};
    auto [x, y, z, w] = v;

    EXPECT_EQ(x, 1);
    EXPECT_EQ(y, 2);
    EXPECT_EQ(z, 3);
    EXPECT_EQ(w, 4);
}
