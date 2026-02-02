/**
 * @file optional_ref_monadic_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.core.containers.optional;
import std;
import retro.core.test.optional;

namespace
{
    inline constexpr int constexpr_fortytwo = 42;
    constexpr int get_int(int)
    {
        return constexpr_fortytwo;
    }
    constexpr retro::Optional<const int &> get_opt_int(int)
    {
        return constexpr_fortytwo;
    }
} // namespace

TEST(OptionalRefMonadicTest, Transform)
{
    // lhs is empty
    retro::Optional<int &> o1;
    auto o1r = o1.transform([](int i) { return i + 2; });
    static_assert((std::is_same<decltype(o1r), retro::Optional<int>>::value));
    EXPECT_TRUE(!o1r);

    // lhs has value
    int forty = 40;
    retro::Optional<int &> o2 = forty;
    auto o2r = o2.transform([](int i) { return i + 2; });
    static_assert((std::is_same<decltype(o2r), retro::Optional<int>>::value));
    EXPECT_TRUE(o2r.value() == 42);

    struct rval_call_transform
    {
        double operator()(int) &&
        {
            return 42.0;
        };
    };

    // ensure that function object is forwarded
    int fortytwo = 42;
    retro::Optional<int &> o3 = fortytwo;
    auto o3r = o3.transform(rval_call_transform{});
    static_assert((std::is_same<decltype(o3r), retro::Optional<double>>::value));
    EXPECT_TRUE(o3r.value() == 42);

    // // ensure that lhs is forwarded
    // forty = 40;
    // retro::Optional<int&> o4 = forty;
    // auto o4r = std::move(o4).transform([](int &&i) { return i + 2; });
    // static_assert((std::is_same<decltype(o4r), retro::Optional<int&>>::value));
    // EXPECT_TRUE(o4r.value() == 42);

    // ensure that lhs is const-propagated
    forty = 40;
    const retro::Optional<int &> o5 = forty;
    auto o5r = o5.transform([](const int &i) { return i + 2; });
    static_assert((std::is_same<decltype(o5r), retro::Optional<int>>::value));
    EXPECT_TRUE(o5r.value() == 42);

    // test each overload in turn
    fortytwo = 42;
    retro::Optional<int &> o8 = fortytwo;
    auto o8r = o8.transform([](int) { return 42; });
    EXPECT_TRUE(*o8r == 42);

    retro::Optional<int &> o12 = fortytwo;
    auto o12r = std::move(o12).transform([](int) { return 42; });
    EXPECT_TRUE(*o12r == 42);

    const retro::Optional<int &> o16 = fortytwo;
    auto o16r = o16.transform([](int) { return 42; });
    EXPECT_TRUE(*o16r == 42);

    const retro::Optional<int &> o20 = fortytwo;
    auto o20r = std::move(o20).transform([](int) { return 42; });
    EXPECT_TRUE(*o20r == 42);

    retro::Optional<int &> o24 = std::nullopt;
    auto o24r = o24.transform([](int) { return 42; });
    EXPECT_TRUE(!o24r);

    retro::Optional<int &> o28 = std::nullopt;
    auto o28r = std::move(o28).transform([](int) { return 42; });
    EXPECT_TRUE(!o28r);

    const retro::Optional<int &> o32 = std::nullopt;
    auto o32r = o32.transform([](int) { return 42; });
    EXPECT_TRUE(!o32r);

    const retro::Optional<int &> o36 = std::nullopt;
    auto o36r = std::move(o36).transform([](int) { return 42; });
    EXPECT_TRUE(!o36r);

    // callable which returns a reference
    retro::Optional<int &> o38 = fortytwo;
    auto o38r = o38.transform([](int &i) -> const int & { return i; });
    EXPECT_TRUE(o38r);
    EXPECT_TRUE(*o38r == 42);

    // transform and return a non-movable class
    using immovable = retro::tests::immovable;

    retro::Optional<int &> o39 = fortytwo;
    auto o39r = o39.transform([](int &) { return immovable(); });
    EXPECT_TRUE(o39r);

    retro::Optional<int &> o40 = fortytwo;
    auto o40r = std::move(o40).transform([](int &) { return immovable(); });
    EXPECT_TRUE(o40r);

    const retro::Optional<int &> o41 = fortytwo;
    auto o41r = o41.transform([](int &) { return immovable(); });
    EXPECT_TRUE(o41r);

    const retro::Optional<int &> o42 = fortytwo;
    auto o42r = std::move(o42).transform([](int &) { return immovable(); });
    EXPECT_TRUE(o42r);
}

TEST(OptionalRefMonadicTest, TransformConstexpr)
{

    // test each overload in turn
    constexpr retro::Optional<const int &> o16 = constexpr_fortytwo;
    constexpr auto o16r = o16.transform(get_int);
    static_assert(*o16r == 42);

    constexpr retro::Optional<const int &> o20 = constexpr_fortytwo;
    constexpr auto o20r = std::move(o20).transform(get_int);
    static_assert(*o20r == 42);

    constexpr retro::Optional<int &> o32 = std::nullopt;
    constexpr auto o32r = o32.transform(get_int);
    static_assert(!o32r);
    constexpr retro::Optional<int &> o36 = std::nullopt;
    constexpr auto o36r = std::move(o36).transform(get_int);
    static_assert(!o36r);
}

TEST(OptionalRefMonadicTest, Transform2)
{
    // lhs is empty
    retro::Optional<int &> o1;
    auto o1r = o1.transform([](int i) { return i + 2; });
    static_assert((std::is_same<decltype(o1r), retro::Optional<int>>::value));
    EXPECT_TRUE(!o1r);

    // lhs has value
    int forty = 40;
    retro::Optional<int &> o2 = forty;
    auto o2r = o2.transform([](int i) { return i + 2; });
    static_assert((std::is_same<decltype(o2r), retro::Optional<int>>::value));
    EXPECT_TRUE(o2r.value() == 42);

    struct rval_call_transform
    {
        double operator()(int) &&
        {
            return 42.0;
        };
    };

    // ensure that function object is forwarded
    int fortytwo = 42;
    retro::Optional<int &> o3 = fortytwo;
    auto o3r = o3.transform(rval_call_transform{});
    static_assert((std::is_same<decltype(o3r), retro::Optional<double>>::value));
    EXPECT_TRUE(o3r.value() == 42);

    // // ensure that lhs is forwarded
    // int forty = 40;
    // retro::Optional<int&> o4 = forty;
    // auto o4r = std::move(o4).transform([](int&& i) { return i + 2; });
    // static_assert((std::is_same<decltype(o4r), retro::Optional<int&>>::value));
    // EXPECT_TRUE(o4r.value() == 42);

    // ensure that lhs is const-propagated
    forty = 40;
    const retro::Optional<int &> o5 = forty;
    auto o5r = o5.transform([](const int &i) { return i + 2; });
    static_assert((std::is_same<decltype(o5r), retro::Optional<int>>::value));
    EXPECT_TRUE(o5r.value() == 42);

    // test each overload in turn
    fortytwo = 42;
    retro::Optional<int &> o8 = fortytwo;
    auto o8r = o8.transform([](int) { return 42; });
    EXPECT_TRUE(*o8r == 42);

    retro::Optional<int &> o12 = fortytwo;
    auto o12r = std::move(o12).transform([](int) { return 42; });
    EXPECT_TRUE(*o12r == 42);

    const retro::Optional<int &> o16 = fortytwo;
    auto o16r = o16.transform([](int) { return 42; });
    EXPECT_TRUE(*o16r == 42);

    const retro::Optional<int &> o20 = fortytwo;
    auto o20r = std::move(o20).transform([](int) { return 42; });
    EXPECT_TRUE(*o20r == 42);

    retro::Optional<int &> o24 = std::nullopt;
    auto o24r = o24.transform([](int) { return 42; });
    EXPECT_TRUE(!o24r);

    retro::Optional<int &> o28 = std::nullopt;
    auto o28r = std::move(o28).transform([](int) { return 42; });
    EXPECT_TRUE(!o28r);

    const retro::Optional<int &> o32 = std::nullopt;
    auto o32r = o32.transform([](int) { return 42; });
    EXPECT_TRUE(!o32r);

    const retro::Optional<int &> o36 = std::nullopt;
    auto o36r = std::move(o36).transform([](int) { return 42; });
    EXPECT_TRUE(!o36r);
}

TEST(OptionalRefMonadicTest, TransformConstxpr)
{
    // test each overload in turn
    constexpr retro::Optional<const int &> o16 = constexpr_fortytwo;
    constexpr auto o16r = o16.transform(get_int);
    static_assert(*o16r == 42);

    constexpr retro::Optional<const int &> o20 = constexpr_fortytwo;
    constexpr auto o20r = std::move(o20).transform(get_int);
    static_assert(*o20r == 42);

    constexpr retro::Optional<const int &> o32 = std::nullopt;
    constexpr auto o32r = o32.transform(get_int);
    static_assert(!o32r);
    constexpr retro::Optional<int &> o36 = std::nullopt;
    constexpr auto o36r = std::move(o36).transform(get_int);
    static_assert(!o36r);
}

TEST(OptionalRefMonadicTest, and_then)
{
    // lhs is empty
    retro::Optional<int &> o1;
    auto o1r = o1.and_then([](int) { return retro::Optional<float>{42}; });
    static_assert((std::is_same<decltype(o1r), retro::Optional<float>>::value));
    EXPECT_TRUE(!o1r);

    // lhs has value
    int twelve = 12;
    retro::Optional<int &> o2 = twelve;
    auto o2r = o2.and_then([](int) { return retro::Optional<float>{42}; });
    static_assert((std::is_same<decltype(o2r), retro::Optional<float>>::value));
    EXPECT_TRUE(o2r.value() == 42.f);

    // lhs is empty, rhs returns empty
    retro::Optional<int &> o3;
    auto o3r = o3.and_then([](int) { return retro::Optional<float>{}; });
    static_assert((std::is_same<decltype(o3r), retro::Optional<float>>::value));
    EXPECT_TRUE(!o3r);

    // rhs returns empty
    retro::Optional<int &> o4 = twelve;
    auto o4r = o4.and_then([](int) { return retro::Optional<float>{}; });
    static_assert((std::is_same<decltype(o4r), retro::Optional<float>>::value));
    EXPECT_TRUE(!o4r);

    struct rval_call_and_then
    {
        retro::Optional<double> operator()(int) &&
        {
            return retro::Optional<double>(42.0);
        };
    };

    // ensure that function object is forwarded
    int fortytwo = 42;
    retro::Optional<int &> o5 = fortytwo;
    auto o5r = o5.and_then(rval_call_and_then{});
    static_assert((std::is_same<decltype(o5r), retro::Optional<double>>::value));
    EXPECT_TRUE(o5r.value() == 42);

    // // ensure that lhs is forwarded
    // retro::Optional<int&> o6 = fortytwo;
    // auto o6r =
    //     std::move(o6).and_then([](int &&i) { return retro::Optional<double>(i); });
    // static_assert((std::is_same<decltype(o6r), retro::Optional<double>>::value));
    // EXPECT_TRUE(o6r.value() == 42);

    // ensure that function object is const-propagated
    const retro::Optional<int &> o7 = fortytwo;
    auto o7r = o7.and_then([](const int &i) { return retro::Optional<double>(i); });
    static_assert((std::is_same<decltype(o7r), retro::Optional<double>>::value));
    EXPECT_TRUE(o7r.value() == 42);

    // test each overload in turn
    retro::Optional<int &> o8 = fortytwo;
    auto o8r = o8.and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(*o8r == 42);

    retro::Optional<int &> o9 = fortytwo;
    auto o9r = std::move(o9).and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(*o9r == 42);

    const retro::Optional<int &> o10 = fortytwo;
    auto o10r = o10.and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(*o10r == 42);

    const retro::Optional<int &> o11 = fortytwo;
    auto o11r = std::move(o11).and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(*o11r == 42);

    retro::Optional<int &> o16 = std::nullopt;
    auto o16r = o16.and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(!o16r);

    retro::Optional<int &> o17 = std::nullopt;
    auto o17r = std::move(o17).and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(!o17r);

    const retro::Optional<int &> o18 = std::nullopt;
    auto o18r = o18.and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(!o18r);

    const retro::Optional<int &> o19 = std::nullopt;
    auto o19r = std::move(o19).and_then([](int) { return retro::make_optional(42); });
    EXPECT_TRUE(!o19r);

    int i = 3;
    retro::Optional<int &> o20{i};
    std::move(o20).and_then([](int &r) { return retro::Optional<int &>{++r}; });
    EXPECT_TRUE(o20);
    EXPECT_TRUE(i == 4);
}

TEST(OptionalRefMonadicTest, Constexpr_and_then)
{
    constexpr retro::Optional<const int &> o10 = constexpr_fortytwo;
    constexpr auto o10r = o10.and_then(get_opt_int);
    EXPECT_TRUE(*o10r == 42);

    constexpr retro::Optional<const int &> o11 = constexpr_fortytwo;
    constexpr auto o11r = std::move(o11).and_then(get_opt_int);
    EXPECT_TRUE(*o11r == 42);

    constexpr retro::Optional<int &> o18 = std::nullopt;
    constexpr auto o18r = o18.and_then(get_opt_int);
    EXPECT_TRUE(!o18r);

    constexpr retro::Optional<int &> o19 = std::nullopt;
    constexpr auto o19r = std::move(o19).and_then(get_opt_int);
    EXPECT_TRUE(!o19r);
}

TEST(OptionalRefMonadicTest, or_else)
{
    int fortytwo = 42;
    int thirteen = 13;
    retro::Optional<int &> o1 = fortytwo;
    EXPECT_TRUE(*(o1.or_else([&] { return retro::Optional<int &>(thirteen); })) == 42);

    retro::Optional<int &> o2;
    EXPECT_EQ(*(o2.or_else([&] { return retro::Optional<int &>(thirteen); })), 13);
}
