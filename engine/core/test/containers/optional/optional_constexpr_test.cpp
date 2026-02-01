/**
 * @file optional_constexpr_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "test_utilities.hpp"

#include <gtest/gtest.h>

import retro.core;
import std;
import retro.core.test.optional;

TEST(OptionalConstexprTest, Constructors)
{
    constexpr retro::Optional<int> i1;
    constexpr retro::Optional<int> i2{std::nullopt};
    std::ignore = i1;
    std::ignore = i2;

    constexpr int i = 0;
    constexpr retro::Optional<int> i3 = i;
    std::ignore = i3;

    constexpr retro::Optional<retro::tests::empty> e1;
    constexpr retro::Optional<int> e2{std::nullopt};

    constexpr retro::tests::empty e{};
    constexpr retro::Optional<retro::tests::empty> e3 = e;
    std::ignore = e1;
    std::ignore = e2;
    std::ignore = e;
    std::ignore = e3;
}

TEST(OptionalConstexprTest, Constructors2)
{
    constexpr retro::Optional<int> o1;
    EXPECT_TRUE(!o1);

    constexpr retro::Optional<int> o2 = std::nullopt;
    EXPECT_TRUE(!o2);

    constexpr retro::Optional<int> o3 = 42;
    EXPECT_TRUE(*o3 == 42);

    constexpr retro::Optional<int> o4 = o3;
    EXPECT_TRUE(*o4 == 42);

    constexpr retro::Optional<int> o5 = o1;
    EXPECT_TRUE(!o5);

    constexpr retro::Optional<int> o6 = std::move(o3);
    EXPECT_TRUE(*o6 == 42);

    constexpr retro::Optional<short> o7 = 42;
    EXPECT_TRUE(*o7 == 42);

    constexpr retro::Optional<int> o8 = o7;
    EXPECT_TRUE(*o8 == 42);

    constexpr retro::Optional<int> o9 = std::move(o7);
    EXPECT_TRUE(*o9 == 42);

    {
        constexpr retro::Optional<int &> o;
        EXPECT_TRUE(!o);

        constexpr retro::Optional<int &> oo = o;
        EXPECT_TRUE(!oo);
    }

    {
        static constexpr auto i = 42;
        constexpr retro::Optional<const int &> o = i;
        EXPECT_TRUE(o);
        EXPECT_TRUE(*o == 42);

        constexpr retro::Optional<const int &> oo = o;
        EXPECT_TRUE(oo);
        EXPECT_TRUE(*oo == 42);
    }
}

TEST(OptionalConstexprTest, Constructors3)
{
    constexpr retro::Optional<int> ie;
    constexpr retro::Optional<int> i4 = ie;
    EXPECT_FALSE(i4);

    using retro::tests::base;
    using retro::tests::derived;

    constexpr base b{1};
    constexpr derived d(1, 2);
    constexpr retro::Optional<base> b1{b};
    constexpr retro::Optional<base> b2{d};

    constexpr retro::Optional<derived> d2{d};
    constexpr retro::Optional<base> b3 = d2;
    constexpr retro::Optional<base> b4{d2};
    std::ignore = b1;
    std::ignore = b2;
    std::ignore = b3;
    std::ignore = b4;
    std::ignore = d2;
}

namespace
{
    class NoDefault
    {
        int v_;

      public:
        constexpr NoDefault(int v) : v_(v)
        {
        }
        constexpr int value() const
        {
            return v_;
        }
    };
} // namespace

TEST(OptionalConstexprTest, NonDefaultConstruct)
{
    constexpr NoDefault i = 7;
    constexpr retro::Optional<NoDefault> v1{};
    constexpr retro::Optional<NoDefault> v2{i};
    std::ignore = v1;
    std::ignore = v2;
}

consteval bool testConstexprAssignmentValue()
{
    bool retval = true;
    retro::Optional<int> o1 = 42;
    retro::Optional<int> o2 = 12;
    retro::Optional<int> o3;

    o1 = static_cast<retro::Optional<int> &>(o1);
    retval &= (*o1 == 42);

    o1 = o2;
    retval &= (*o1 == 12);

    o1 = o3;
    retval &= (!o1);

    o1 = 42;
    retval &= (*o1 == 42);

    o1 = std::nullopt;
    retval &= (!o1);

    o1 = std::move(o2);
    retval &= (*o1 == 12);

    retro::Optional<short> o4 = 42;

    o1 = o4;
    retval &= (*o1 == 42);

    o1 = std::move(o4);
    retval &= (*o1 == 42);

    /*
      template <class U = T>
      constexpr optional& operator=(U&& u)
    */
    short s = 54;
    o1 = s;
    retval &= (*o1 == 54);

    retro::Optional<short> emptyShort;
    o1 = emptyShort;
    retval &= !o1.has_value();

    o1 = o4;
    o1 = std::move(emptyShort);
    retval &= !o1.has_value();

    struct not_trivial_copy_assignable
    {
        int i_;
        constexpr not_trivial_copy_assignable(int i) : i_(i)
        {
        }
        constexpr not_trivial_copy_assignable(const not_trivial_copy_assignable &) = default;
        constexpr not_trivial_copy_assignable &operator=(const not_trivial_copy_assignable &rhs)
        {
            i_ = rhs.i_;
            return *this;
        }
    };

    /*
      optional& operator=(const optional& rhs)
      requires std::is_copy_constructible_v<T> && std::is_copy_assignable_v<T> &&
      (!std::is_trivially_copy_assignable_v<T>)
    */
    retro::Optional<not_trivial_copy_assignable> o5{5};
    retro::Optional<not_trivial_copy_assignable> o6;
    o6 = o5;
    retval &= (o5->i_ == 5);
    return retval;
}

static_assert(testConstexprAssignmentValue());

struct takes_init_and_variadic
{
    int v0;
    std::tuple<int, int> t;
    template <class... Args>
    constexpr takes_init_and_variadic(std::initializer_list<int> &l, Args &&...args)
        : v0{*std::begin(l)}, t(std::forward<Args>(args)...)
    {
    }
};

consteval bool testConstexprInPlace()
{
    bool retval = true;
    constexpr retro::Optional<int> o1{std::in_place};
    constexpr retro::Optional<int> o2(std::in_place);
    retval &= (bool(o1));
    retval &= (o1 == 0);
    retval &= (bool(o2));
    retval &= (o2 == 0);

    constexpr retro::Optional<int> o3(std::in_place, 42);
    retval &= (o3 == 42);

    constexpr retro::Optional<std::tuple<int, int>> o4(std::in_place, 0, 1);
    retval &= (bool(o4));
    retval &= (std::get<0>(*o4) == 0);
    retval &= (std::get<1>(*o4) == 1);

    constexpr retro::Optional<takes_init_and_variadic> o6(std::in_place, {0, 1}, 2, 3);
    retval &= (o6->v0 == 0);
    retval &= (std::get<0>(o6->t) == 2);
    retval &= (std::get<1>(o6->t) == 3);
    return retval;
}

using retro::tests::constify;

TEST(OptionalConstexprTest, InPlace)
{
    EXPECT_TRUE(constify(testConstexprInPlace()));
}

TEST(OptionalConstexprTest, MakeOptional)
{
    constexpr auto o1 = retro::make_optional(42);
    constexpr auto o2 = retro::Optional<int>(42);

    constexpr bool is_same = std::is_same<decltype(o1), const retro::Optional<int>>::value;
    EXPECT_TRUE(is_same);
    EXPECT_TRUE(o1 == o2);

    constexpr auto o3 = retro::make_optional<std::tuple<int, int, int, int>>(0, 1, 2, 3);
    EXPECT_TRUE(std::get<0>(*o3) == 0);
    EXPECT_TRUE(std::get<1>(*o3) == 1);
    EXPECT_TRUE(std::get<2>(*o3) == 2);
    EXPECT_TRUE(std::get<3>(*o3) == 3);

    constexpr auto o5 = retro::make_optional<takes_init_and_variadic>({0, 1}, 2, 3);
    EXPECT_TRUE(o5->v0 == 0);
    EXPECT_TRUE(std::get<0>(o5->t) == 2);
    EXPECT_TRUE(std::get<1>(o5->t) == 3);
}

TEST(OptionalConstexprTest, Nullopt)
{
    constexpr retro::Optional<int> o1 = std::nullopt;
    constexpr retro::Optional<int> o2{std::nullopt};
    constexpr retro::Optional<int> o3(std::nullopt);
    constexpr retro::Optional<int> o4 = {std::nullopt};

    EXPECT_TRUE(constify(!o1));
    EXPECT_TRUE(constify(!o2));
    EXPECT_TRUE(constify(!o3));
    EXPECT_TRUE(constify(!o4));

    EXPECT_TRUE(!std::is_default_constructible<std::nullopt_t>::value);
}

TEST(OptionalConstexprTest, Observers)
{
    constexpr retro::Optional<int> o1 = 42;
    constexpr retro::Optional<int> o2;
    constexpr retro::Optional<int> o3 = 42;

    EXPECT_TRUE(*o1 == 42);
    EXPECT_TRUE(*o1 == o1.value());
    EXPECT_TRUE(o2.value_or(42) == 42);
    EXPECT_TRUE(o3.value() == 42);
    constexpr auto success = std::is_same_v<decltype(o1.value()), const int &>;
    EXPECT_TRUE(success);
    constexpr auto success2 = std::is_same_v<decltype(o3.value()), const int &>;
    EXPECT_TRUE(success2);
    constexpr auto success3 = std::is_same_v<decltype(std::move(o1).value()), const int &>;
    EXPECT_TRUE(success3);
}

TEST(OptionalConstexprTest, RelationalOps)
{
    constexpr retro::Optional<int> o1{4};
    constexpr retro::Optional<int> o2{42};
    constexpr retro::Optional<int> o3{};

    //  SECTION("self simple")
    {
        EXPECT_TRUE(constify(!(o1 == o2)));
        EXPECT_TRUE(constify((o1 == o1)));
        EXPECT_TRUE(constify(!(o1 == o2)));
        EXPECT_TRUE(constify(!(o1 != o1)));
        EXPECT_TRUE(constify((o1 < o2)));
        EXPECT_TRUE(constify(!(o1 < o1)));
        EXPECT_TRUE(constify(!(o1 > o2)));
        EXPECT_TRUE(constify(!(o1 > o1)));
        EXPECT_TRUE(constify((o1 <= o2)));
        EXPECT_TRUE(constify((o1 <= o1)));
        EXPECT_TRUE(constify(!(o1 >= o2)));
        EXPECT_TRUE(constify(o1 >= o1));
    }
    //  SECTION("nullopt simple")
    {
        {
            EXPECT_TRUE(constify((!(o1 == std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt == o1))));
            EXPECT_TRUE(constify((o1 != std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt != o1)));
            EXPECT_TRUE(constify((!(o1 < std::nullopt))));
            EXPECT_TRUE(constify((std::nullopt < o1)));
            EXPECT_TRUE(constify((o1 > std::nullopt)));
            EXPECT_TRUE(constify((!(std::nullopt > o1))));
            EXPECT_TRUE(constify((!(o1 <= std::nullopt))));
            EXPECT_TRUE(constify((std::nullopt <= o1)));
            EXPECT_TRUE(constify((o1 >= std::nullopt)));
            EXPECT_TRUE(constify((!(std::nullopt >= o1))));
            EXPECT_TRUE(constify((o3 == std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt == o3)));
            EXPECT_TRUE(constify((!(o3 != std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt != o3))));
            EXPECT_TRUE(constify((!(o3 < std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt < o3))));
            EXPECT_TRUE(constify((!(o3 > std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt > o3))));
            EXPECT_TRUE(constify((o3 <= std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt <= o3)));
            EXPECT_TRUE(constify((o3 >= std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt >= o3)));
        }
    }
    //  SECTION("with T simple")
    {
        {
            EXPECT_TRUE(constify((!(o1 == 1))));
            EXPECT_TRUE(constify((!(1 == o1))));
            EXPECT_TRUE(constify((o1 != 1)));
            EXPECT_TRUE(constify((1 != o1)));
            EXPECT_TRUE(constify((!(o1 < 1))));
            EXPECT_TRUE(constify((1 < o1)));
            EXPECT_TRUE(constify((o1 > 1)));
            EXPECT_TRUE(constify((!(1 > o1))));
            EXPECT_TRUE(constify((!(o1 <= 1))));
            EXPECT_TRUE(constify((1 <= o1)));
            EXPECT_TRUE(constify((o1 >= 1)));
            EXPECT_TRUE(constify((!(1 >= o1))));
        }

        {
            EXPECT_TRUE(constify((o1 == 4)));
            EXPECT_TRUE(constify((4 == o1)));
            EXPECT_TRUE(constify((!(o1 != 4))));
            EXPECT_TRUE(constify((!(4 != o1))));
            EXPECT_TRUE(constify((!(o1 < 4))));
            EXPECT_TRUE(constify((!(4 < o1))));
            EXPECT_TRUE(constify((!(o1 > 4))));
            EXPECT_TRUE(constify((!(4 > o1))));
            EXPECT_TRUE(constify((o1 <= 4)));
            EXPECT_TRUE(constify((4 <= o1)));
            EXPECT_TRUE(constify((o1 >= 4)));
            EXPECT_TRUE(constify((4 >= o1)));
        }
    }

    using retro::tests::Point;

    constexpr Point p4{2, 3};
    constexpr Point p5{3, 4};

    constexpr retro::Optional<Point> o4{p4};
    constexpr retro::Optional<Point> o5{p5};

    //  SECTION("self complex")
    {
        {
            EXPECT_TRUE(constify((!(o4 == o5))));
            EXPECT_TRUE(constify((o4 == o4)));
            EXPECT_TRUE(constify((o4 != o5)));
            EXPECT_TRUE(constify((!(o4 != o4))));
            EXPECT_TRUE(constify((o4 < o5)));
            EXPECT_TRUE(constify((!(o4 < o4))));
            EXPECT_TRUE(constify((!(o4 > o5))));
            EXPECT_TRUE(constify((!(o4 > o4))));
            EXPECT_TRUE(constify((o4 <= o5)));
            EXPECT_TRUE(constify((o4 <= o4)));
            EXPECT_TRUE(constify((!(o4 >= o5))));
            EXPECT_TRUE(constify((o4 >= o4)));
        }
    }
    //  SECTION("nullopt complex")
    {
        {
            EXPECT_TRUE(constify((!(o4 == std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt == o4))));
            EXPECT_TRUE(constify((o4 != std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt != o4)));
            EXPECT_TRUE(constify((!(o4 < std::nullopt))));
            EXPECT_TRUE(constify((std::nullopt < o4)));
            EXPECT_TRUE(constify((o4 > std::nullopt)));
            EXPECT_TRUE(constify((!(std::nullopt > o4))));
            EXPECT_TRUE(constify((!(o4 <= std::nullopt))));
            EXPECT_TRUE(constify((std::nullopt <= o4)));
            EXPECT_TRUE(constify((o4 >= std::nullopt)));
            EXPECT_TRUE(constify((!(std::nullopt >= o4))));
        }

        {
            EXPECT_TRUE(constify((o3 == std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt == o3)));
            EXPECT_TRUE(constify((!(o3 != std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt != o3))));
            EXPECT_TRUE(constify((!(o3 < std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt < o3))));
            EXPECT_TRUE(constify((!(o3 > std::nullopt))));
            EXPECT_TRUE(constify((!(std::nullopt > o3))));
            EXPECT_TRUE(constify((o3 <= std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt <= o3)));
            EXPECT_TRUE(constify((o3 >= std::nullopt)));
            EXPECT_TRUE(constify((std::nullopt >= o3)));
        }
    }

    //  SECTION("with T complex")
    {
        {
            EXPECT_TRUE(constify((!(o4 == Point{}))));
            EXPECT_TRUE(constify((!(Point{} == o4))));
            EXPECT_TRUE(constify((o4 != Point{})));
            EXPECT_TRUE(constify((Point{} != o4)));
            EXPECT_TRUE(constify((!(o4 < Point{}))));
            EXPECT_TRUE(constify((Point{} < o4)));
            EXPECT_TRUE(constify((o4 > Point{})));
            EXPECT_TRUE(constify((!(Point{} > o4))));
            EXPECT_TRUE(constify((!(o4 <= Point{}))));
            EXPECT_TRUE(constify((Point{} <= o4)));
            EXPECT_TRUE(constify((o4 >= Point{})));
            EXPECT_TRUE(constify((!(Point{} >= o4))));
        }

        {
            EXPECT_TRUE(constify((o4 == p4)));
            EXPECT_TRUE(constify((p4 == o4)));
            EXPECT_TRUE(constify((!(o4 != p4))));
            EXPECT_TRUE(constify((!(p4 != o4))));
            EXPECT_TRUE(constify((!(o4 < p4))));
            EXPECT_TRUE(constify((!(p4 < o4))));
            EXPECT_TRUE(constify((!(o4 > p4))));
            EXPECT_TRUE(constify((!(p4 > o4))));
            EXPECT_TRUE(constify((o4 <= p4)));
            EXPECT_TRUE(constify((p4 <= o4)));
            EXPECT_TRUE(constify((o4 >= p4)));
            EXPECT_TRUE(constify((p4 >= o4)));
        }
    }
}

consteval bool testComparisons()
{
    constexpr retro::Optional<int> o1{4};
    constexpr retro::Optional<int> o2{42};
    constexpr retro::Optional<int> o3{};

    //  SECTION("self simple")
    {
        {
            constexpr auto b = !(o1 == o2);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 == o1);
            static_assert(b);
        }

        {
            constexpr auto b = !(o1 == o2);
            static_assert(b);
        }

        {
            constexpr auto b = !(o1 != o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 < o2);
            static_assert(b);
        }

        {
            constexpr auto b = !(o1 < o1);
            static_assert(b);
        }

        {
            constexpr auto b = !(o1 > o2);
            static_assert(b);
        }

        {
            constexpr auto b = !(o1 > o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 <= o2);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 <= o1);
            static_assert(b);
        }

        {
            constexpr auto b = !(o1 >= o2);
            static_assert(b);
        }

        {
            constexpr auto b = o1 >= o1;
            static_assert(b);
        }
    }
    //  SECTION("nullopt simple")
    {
        {
            constexpr auto b = (!(o1 == std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt == o1));
            static_assert(b);
        }

        {
            constexpr auto b = (o1 != std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt != o1);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 < std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt < o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 > std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt > o1));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 <= std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt <= o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 >= std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt >= o1));
            static_assert(b);
        }

        {
            constexpr auto b = (o3 == std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt == o3);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o3 != std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt != o3));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o3 < std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt < o3));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o3 > std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt > o3));
            static_assert(b);
        }

        {
            constexpr auto b = (o3 <= std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt <= o3);
            static_assert(b);
        }

        {
            constexpr auto b = (o3 >= std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt >= o3);
            static_assert(b);
        }
    }
    //  SECTION("with T simple")
    {
        {
            constexpr auto b = (!(o1 == 1));
            static_assert(b);
        }

        {
            constexpr auto b = (!(1 == o1));
            static_assert(b);
        }

        {
            constexpr auto b = (o1 != 1);
            static_assert(b);
        }

        {
            constexpr auto b = (1 != o1);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 < 1));
            static_assert(b);
        }

        {
            constexpr auto b = (1 < o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 > 1);
            static_assert(b);
        }

        {
            constexpr auto b = (!(1 > o1));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 <= 1));
            static_assert(b);
        }

        {
            constexpr auto b = (1 <= o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 >= 1);
            static_assert(b);
        }

        {
            constexpr auto b = (!(1 >= o1));
            static_assert(b);
        }

        {
            constexpr auto b = (o1 == 4);
            static_assert(b);
        }

        {
            constexpr auto b = (4 == o1);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 != 4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(4 != o1));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 < 4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(4 < o1));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o1 > 4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(4 > o1));
            static_assert(b);
        }

        {
            constexpr auto b = (o1 <= 4);
            static_assert(b);
        }

        {
            constexpr auto b = (4 <= o1);
            static_assert(b);
        }

        {
            constexpr auto b = (o1 >= 4);
            static_assert(b);
        }

        {
            constexpr auto b = (4 >= o1);
            static_assert(b);
        }
    }

    using retro::tests::Point;

    constexpr Point p4{2, 3};
    constexpr Point p5{3, 4};

    constexpr retro::Optional<Point> o4{p4};
    constexpr retro::Optional<Point> o5{p5};

    //  SECTION("self complex")
    {
        {
            constexpr auto b = (!(o4 == o5));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 == o4);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 != o5);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 != o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 < o5);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 < o4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 > o5));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 > o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 <= o5);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 <= o4);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 >= o5));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 >= o4);
            static_assert(b);
        }
    }
    //  SECTION("nullopt complex")
    {
        {
            constexpr auto b = (!(o4 == std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt == o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 != std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt != o4);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 < std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt < o4);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 > std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt > o4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 <= std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt <= o4);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 >= std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt >= o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o3 == std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt == o3);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o3 != std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt != o3));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o3 < std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt < o3));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o3 > std::nullopt));
            static_assert(b);
        }

        {
            constexpr auto b = (!(std::nullopt > o3));
            static_assert(b);
        }

        {
            constexpr auto b = (o3 <= std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt <= o3);
            static_assert(b);
        }

        {
            constexpr auto b = (o3 >= std::nullopt);
            static_assert(b);
        }

        {
            constexpr auto b = (std::nullopt >= o3);
            static_assert(b);
        }
    }

    //  SECTION("with T complex")
    {
        {
            constexpr auto b = (!(o4 == Point{}));
            static_assert(b);
        }

        {
            constexpr auto b = (!(Point{} == o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 != Point{});
            static_assert(b);
        }

        {
            constexpr auto b = (Point{} != o4);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 < Point{}));
            static_assert(b);
        }

        {
            constexpr auto b = (Point{} < o4);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 > Point{});
            static_assert(b);
        }

        {
            constexpr auto b = (!(Point{} > o4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 <= Point{}));
            static_assert(b);
        }

        {
            constexpr auto b = (Point{} <= o4);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 >= Point{});
            static_assert(b);
        }

        {
            constexpr auto b = (!(Point{} >= o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 == p4);
            static_assert(b);
        }

        {
            constexpr auto b = (p4 == o4);
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 != p4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(p4 != o4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 < p4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(p4 < o4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(o4 > p4));
            static_assert(b);
        }

        {
            constexpr auto b = (!(p4 > o4));
            static_assert(b);
        }

        {
            constexpr auto b = (o4 <= p4);
            static_assert(b);
        }

        {
            constexpr auto b = (p4 <= o4);
            static_assert(b);
        }

        {
            constexpr auto b = (o4 >= p4);
            static_assert(b);
        }

        {
            constexpr auto b = (p4 >= o4);
            static_assert(b);
        }
    }
    return true;
}

constexpr bool checkTestComparison = testComparisons();
static_assert(checkTestComparison);

TEST(OptionalConstexprTest, RangeTest)
{
    constexpr retro::Optional<int> o1 = std::nullopt;
    constexpr retro::Optional<int> o2 = 42;
    EXPECT_EQ(*o2, 42);
    for (auto k : o1)
    {
        std::ignore = k;
        EXPECT_TRUE(false);
    }
    for (auto k : o2)
    {
        EXPECT_EQ(k, 42);
    }
}

consteval bool testSwap()
{
    retro::Optional<int> o1 = 42;
    retro::Optional<int> o2 = 12;
    o1.swap(o2);
    return (o1.value() == 12) && (o2.value() == 42);
}
static_assert(testSwap());

consteval bool testSwapWNull()
{
    retro::Optional<int> o1 = 42;
    retro::Optional<int> o2 = std::nullopt;
    o1.swap(o2);
    return (!o1.has_value()) && (o2.value(), 42);
}
static_assert(testSwapWNull());

consteval bool testSwapNullIntializedWithValue()
{
    retro::Optional<int> o1 = std::nullopt;
    retro::Optional<int> o2 = 42;
    o1.swap(o2);
    return (o1.value() == 42) && (!o2.has_value());
}
static_assert(testSwapNullIntializedWithValue());

consteval bool testEmplace()
{
    retro::Optional<std::pair<std::pair<int, int>, std::pair<double, double>>> i;
    i.emplace(std::piecewise_construct, std::make_tuple(0, 2), std::make_tuple(3, 4));
    return (i->first.first == 0) && (i->first.second == 2) && (i->second.first == 3) && (i->second.second == 4);
}
static_assert(testEmplace());

consteval bool testEmplaceInitList()
{
    retro::Optional<takes_init_and_variadic> o;
    o.emplace({0, 1}, 2, 3);
    return (o->v0 == 0) && (std::get<0>(o->t) == 2) && (std::get<1>(o->t) == 3);
}
static_assert(testEmplaceInitList());

consteval bool testAssignment()
{
    retro::Optional<int> o1 = 42;
    retro::Optional<int> o2 = 12;

    bool retval = true;

    o2 = std::move(o1);
    retval &= (*o1 == 42);

    o1 = {};

    return retval;
}
static_assert(testAssignment());

consteval bool testAssignmentValue()
{
    retro::Optional<int> o1 = 42;
    retro::Optional<int> o2 = 12;
    retro::Optional<int> o3;

    bool retval = true;

    retval &= (*o1 == 42);

    o1 = o2;
    retval &= (*o1 == 12);

    o1 = o3;
    retval &= (!o1);

    o1 = 42;
    retval &= (*o1 == 42);

    o1 = std::nullopt;
    retval &= (!o1);

    o1 = std::move(o2);
    retval &= (*o1 == 12);

    retro::Optional<short> o4 = 42;

    o1 = o4;
    retval &= (*o1 == 42);

    o1 = std::move(o4);
    retval &= (*o1 == 42);

    short s = 54;
    o1 = s;
    retval &= (*o1 == 54);

    struct not_trivial_copy_assignable
    {
        int i_;
        constexpr not_trivial_copy_assignable(int i) : i_(i)
        {
        }
        constexpr not_trivial_copy_assignable(const not_trivial_copy_assignable &) = default;
        constexpr not_trivial_copy_assignable &operator=(const not_trivial_copy_assignable &rhs)
        {
            i_ = rhs.i_;
            return *this;
        }
    };
    static_assert(!std::is_trivially_copy_assignable_v<not_trivial_copy_assignable>);

    /*
      optional& operator=(const optional& rhs)
        requires std::is_copy_constructible_v<T> && std::is_copy_assignable_v<T> &&
        (!std::is_trivially_copy_assignable_v<T>)
    */
    retro::Optional<not_trivial_copy_assignable> o5{5};
    retro::Optional<not_trivial_copy_assignable> o6;
    o6 = o5;
    retval &= (o5->i_ == 5);

    return retval;
}

static_assert(testAssignmentValue());
