/**
 * @file test_small_unique_ptr.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.core.memory.small_unique_ptr;
import std;
import retro.core.type_traits.basic;
import retro.core.type_traits.pointer;

namespace
{
    struct SmallPOD
    {
        char dummy_;
    };
    struct LargePOD
    {
        char dummy_[128];
    };

    struct Base
    {
        constexpr virtual int value() const
        {
            return 0;
        }
        constexpr virtual int padding() const
        {
            return 0;
        }
        constexpr virtual ~Base() noexcept {};
    };

    template <size_t Padding>
    struct Derived : Base
    {
      public:
        constexpr Derived() = default;
        constexpr Derived(int n) noexcept : value_(n)
        {
        }
        constexpr ~Derived() noexcept override
        {
        }
        constexpr int value() const override
        {
            return value_;
        }
        constexpr int padding() const override
        {
            return Padding;
        }

      private:
        unsigned char padding_[Padding] = {};
        int value_ = Padding;
    };

    using SmallDerived = Derived<32>;
    using LargeDerived = Derived<64>;

    struct BaseIntrusive
    {
        constexpr virtual int value() const
        {
            return 0;
        }
        constexpr virtual int padding() const
        {
            return 0;
        }
        constexpr virtual ~BaseIntrusive() noexcept {};

        virtual void small_unique_ptr_move(void *dst) noexcept
        {
            std::construct_at(static_cast<BaseIntrusive *>(dst), std::move(*this));
        }
    };

    template <size_t Padding>
    struct DerivedIntrusive : BaseIntrusive
    {
      public:
        constexpr DerivedIntrusive() = default;
        constexpr DerivedIntrusive(int n) noexcept : value_(n)
        {
        }
        constexpr ~DerivedIntrusive() noexcept override
        {
        }
        constexpr int value() const override
        {
            return value_;
        }
        constexpr int padding() const override
        {
            return Padding;
        }

        void small_unique_ptr_move(void *dst) noexcept override
        {
            std::construct_at(static_cast<DerivedIntrusive *>(dst), std::move(*this));
        }

      private:
        unsigned char padding_[Padding] = {};
        int value_ = Padding;
    };

    using SmallIntrusive = DerivedIntrusive<32>;
    using LargeIntrusive = DerivedIntrusive<64>;
} // namespace

TEST(RemoveMe, cv_qual_rank)
{
    static_assert(retro::cv_qual_rank<void> == 0);

    static_assert(retro::cv_qual_rank<int> == 0);
    static_assert(retro::cv_qual_rank<const int> == 1);
    static_assert(retro::cv_qual_rank<volatile int> == 1);
    static_assert(retro::cv_qual_rank<const volatile int> == 2);

    static_assert(retro::cv_qual_rank<const int *> == 0);
    static_assert(retro::cv_qual_rank<int *const> == 1);
}

TEST(RemoveMe, is_less_cv_qualified)
{
    static_assert(retro::LessCvQualified<int, const int>);
    static_assert(retro::LessCvQualified<int, volatile int>);
    static_assert(retro::LessCvQualified<const int, const volatile int>);

    static_assert(!retro::LessCvQualified<int, int>);
    static_assert(!retro::LessCvQualified<const int, const int>);
    static_assert(!retro::LessCvQualified<const int, volatile int>);
}

TEST(RemoveMe, is_proper_base_of)
{
    static_assert(retro::ProperBaseOf<Base, SmallDerived>);
    static_assert(retro::ProperBaseOf<const Base, SmallDerived>);

    static_assert(!retro::ProperBaseOf<Base, Base>);
    static_assert(!retro::ProperBaseOf<int, int>);
    static_assert(!retro::ProperBaseOf<void, int>);
}

TEST(RemoveMe, is_pointer_convertible)
{
    static_assert(retro::PointerConvertible<int, int>);
    static_assert(retro::PointerConvertible<int, const int>);

    static_assert(!retro::PointerConvertible<const int, int>);
    static_assert(!retro::PointerConvertible<int, void>);

    static_assert(retro::PointerConvertible<SmallDerived, Base>);
    static_assert(retro::PointerConvertible<SmallDerived, const Base>);

    static_assert(retro::PointerConvertible<Base, Base>);
    static_assert(retro::PointerConvertible<Base, const Base>);

    static_assert(!retro::PointerConvertible<Base, SmallDerived>);
    static_assert(!retro::PointerConvertible<const Base, Base>);
    static_assert(!retro::PointerConvertible<int, Base>);
}

template <typename T>
class SmallUniquePtrBaseTypedTest : public ::testing::Test
{
};
using SmallUniquePtrBaseTypes = ::testing::Types<Base, BaseIntrusive>;
TYPED_TEST_SUITE(SmallUniquePtrBaseTypedTest, SmallUniquePtrBaseTypes);

template <typename T>
class SmallUniquePtrScalarTypedTest : public ::testing::Test
{
};
using SmallUniquePtrScalarTypes = ::testing::
    Types<SmallPOD, LargePOD, Base, SmallDerived, LargeDerived, BaseIntrusive, SmallIntrusive, LargeIntrusive>;
TYPED_TEST_SUITE(SmallUniquePtrScalarTypedTest, SmallUniquePtrScalarTypes);

template <typename T>
class SmallUniquePtrArrayTypedTest : public ::testing::Test
{
};
using SmallUniquePtrArrayTypes = ::testing::Types<SmallPOD, LargePOD>;
TYPED_TEST_SUITE(SmallUniquePtrArrayTypedTest, SmallUniquePtrArrayTypes);

template <typename T>
class SmallUniquePtrBoolConvTypedTest : public ::testing::Test
{
};
using SmallUniquePtrBoolConvTypes = ::testing::Types<Base, SmallDerived, LargeDerived, SmallPOD, LargePOD>;
TYPED_TEST_SUITE(SmallUniquePtrBoolConvTypedTest, SmallUniquePtrBoolConvTypes);

template <typename T>
class SmallUniquePtrGetTypedTest : public ::testing::Test
{
};
using SmallUniquePtrGetTypes = ::testing::Types<Base, SmallDerived, LargeDerived, SmallPOD, LargePOD>;
TYPED_TEST_SUITE(SmallUniquePtrGetTypedTest, SmallUniquePtrGetTypes);

TEST(SmallUniquePtr, ObjectLayout)
{
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<int>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<SmallPOD>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<LargePOD>>);

    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<SmallPOD[]>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<LargePOD[]>>);

    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<Base>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<SmallDerived>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<LargeDerived>>);

    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<BaseIntrusive>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<SmallIntrusive>>);
    static_assert(std::is_standard_layout_v<retro::SmallUniquePtr<LargeIntrusive>>);
}

TEST(SmallUniquePtr, ObjectSizeDefault)
{
    static_assert(sizeof(retro::SmallUniquePtr<SmallPOD>) <= 2 * sizeof(void *));
    static_assert(sizeof(retro::SmallUniquePtr<LargePOD>) == sizeof(void *));

    static_assert(alignof(retro::SmallUniquePtr<SmallPOD>) == alignof(void *));
    static_assert(alignof(retro::SmallUniquePtr<LargePOD>) == alignof(void *));

    static_assert(sizeof(retro::SmallUniquePtr<SmallPOD[]>) == retro::default_small_ptr_size);
    static_assert(sizeof(retro::SmallUniquePtr<LargePOD[]>) == sizeof(void *));

    static_assert(alignof(retro::SmallUniquePtr<SmallPOD[]>) == alignof(void *));
    static_assert(alignof(retro::SmallUniquePtr<LargePOD[]>) == alignof(void *));

    static_assert(sizeof(retro::SmallUniquePtr<SmallDerived>) == retro::default_small_ptr_size);
    static_assert(sizeof(retro::SmallUniquePtr<LargeDerived>) == sizeof(void *));

    static_assert(alignof(retro::SmallUniquePtr<SmallDerived>) == retro::default_small_ptr_size);
    static_assert(alignof(retro::SmallUniquePtr<LargeDerived>) == alignof(void *));

    static_assert(sizeof(retro::SmallUniquePtr<SmallIntrusive>) == retro::default_small_ptr_size);
    static_assert(sizeof(retro::SmallUniquePtr<LargeIntrusive>) == sizeof(void *));

    static_assert(alignof(retro::SmallUniquePtr<SmallIntrusive>) == retro::default_small_ptr_size);
    static_assert(alignof(retro::SmallUniquePtr<LargeIntrusive>) == alignof(void *));
}

TYPED_TEST(SmallUniquePtrBaseTypedTest, ObjectSizeCustom)
{
    using TestType = TypeParam;

    static_assert(sizeof(retro::SmallUniquePtr<TestType, 8>) == 8);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 16>) <=
                  16); // Base will be always heap allocated on 64 bit arch
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 24>) == 24);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 32>) == 32);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 40>) == 40);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 48>) == 48);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 56>) == 56);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 64>) == 64);
    static_assert(sizeof(retro::SmallUniquePtr<TestType, 128>) == 128);
}

TYPED_TEST(SmallUniquePtrBaseTypedTest, ObjectAlignCustom)
{
    using TestType = TypeParam;

    static_assert(alignof(retro::SmallUniquePtr<TestType, 8>) == 8);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 16>) <=
                  16); // Base will be always heap allocated on 64 bit arch
    static_assert(alignof(retro::SmallUniquePtr<TestType, 24>) == 8);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 32>) == 32);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 40>) == 8);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 48>) == 16);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 56>) == 8);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 64>) == 64);
    static_assert(alignof(retro::SmallUniquePtr<TestType, 128>) == 128);
}

TEST(SmallUniquePtr, ObjectAlignCustomPod)
{
    static_assert(alignof(SmallPOD) <= alignof(SmallPOD *));

    static_assert(alignof(retro::SmallUniquePtr<SmallPOD, 8>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD, 16>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD, 32>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD, 64>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD, 128>) == alignof(SmallPOD *));

    static_assert(alignof(retro::SmallUniquePtr<SmallPOD[], 8>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD[], 16>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD[], 32>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD[], 64>) == alignof(SmallPOD *));
    static_assert(alignof(retro::SmallUniquePtr<SmallPOD[], 128>) == alignof(SmallPOD *));
}

TEST(SmallUniquePtr, StackBufferSize)
{
    static_assert(retro::SmallUniquePtr<SmallPOD>::stack_buffer_size() == sizeof(SmallPOD));
    static_assert(retro::SmallUniquePtr<LargePOD>::stack_buffer_size() == 0);

    static_assert(retro::SmallUniquePtr<SmallPOD[]>::stack_buffer_size() != 0);
    static_assert(retro::SmallUniquePtr<LargePOD[]>::stack_buffer_size() == 0);

    static_assert(retro::SmallUniquePtr<Base>::stack_buffer_size() != 0);
    static_assert(retro::SmallUniquePtr<BaseIntrusive>::stack_buffer_size() != 0);

    static_assert(retro::SmallUniquePtr<LargeDerived>::stack_buffer_size() == 0);
    static_assert(retro::SmallUniquePtr<LargeIntrusive>::stack_buffer_size() == 0);

    static_assert(retro::SmallUniquePtr<BaseIntrusive>::stack_buffer_size() >
                  retro::SmallUniquePtr<Base>::stack_buffer_size());
    static_assert(retro::SmallUniquePtr<SmallIntrusive>::stack_buffer_size() >
                  retro::SmallUniquePtr<SmallDerived>::stack_buffer_size());

    if (sizeof(void *) == 8)
    {
        ASSERT_EQ(retro::SmallUniquePtr<Base>::stack_buffer_size(), 48);
        ASSERT_EQ(retro::SmallUniquePtr<BaseIntrusive>::stack_buffer_size(), 56);

        ASSERT_EQ(retro::SmallUniquePtr<SmallDerived>::stack_buffer_size(), 48);
        ASSERT_EQ(retro::SmallUniquePtr<SmallIntrusive>::stack_buffer_size(), 56);

        ASSERT_EQ(retro::SmallUniquePtr<SmallPOD[]>::stack_buffer_size(), 56);

        ASSERT_EQ(retro::SmallUniquePtr<SmallPOD[]>::stack_array_size(), 56);
        ASSERT_EQ(retro::SmallUniquePtr<LargePOD[]>::stack_array_size(), 0);

        ASSERT_GT(retro::SmallUniquePtr<SmallDerived[]>::stack_array_size(), 0u);
    }
}

TYPED_TEST(SmallUniquePtrScalarTypedTest, ConstructScalar)
{
    using TestType = TypeParam;

    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<TestType>();
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<const TestType>();
            return true;
        }));

    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<TestType>(nullptr);
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<const TestType>(nullptr);
            return true;
        }));
}

TYPED_TEST(SmallUniquePtrScalarTypedTest, MakeUniqueScalar)
{
    using TestType = TypeParam;

    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small<TestType>();
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small<const TestType>();
            return true;
        }));

    const auto p1 = retro::make_unique_small<TestType>();
    const auto p2 = retro::make_unique_small<const TestType>();
    ASSERT_TRUE(static_cast<bool>(p1));
    ASSERT_TRUE(static_cast<bool>(p2));
}

TEST(SmallUniquePtr, MakeUniqueCast)
{
    static_assert(std::invoke(
        []
        {
            auto p = retro::make_unique_small<SmallDerived, Base>();
            return p;
        }));
    static_assert(std::invoke(
        []
        {
            auto p = retro::make_unique_small<LargeDerived, Base>();
            return p;
        }));

    static_assert(std::invoke(
        []
        {
            auto p = retro::make_unique_small<SmallIntrusive, BaseIntrusive>();
            return p;
        }));
    static_assert(std::invoke(
        []
        {
            auto p = retro::make_unique_small<LargeIntrusive, BaseIntrusive>();
            return p;
        }));

    const auto p1 = retro::make_unique_small<SmallDerived, Base>();
    const auto p2 = retro::make_unique_small<LargeDerived, Base>();
    ASSERT_TRUE(static_cast<bool>(p1));
    ASSERT_TRUE(static_cast<bool>(p2));

    const auto p3 = retro::make_unique_small<SmallIntrusive, BaseIntrusive>();
    const auto p4 = retro::make_unique_small<LargeIntrusive, BaseIntrusive>();
    ASSERT_TRUE(static_cast<bool>(p3));
    ASSERT_TRUE(static_cast<bool>(p4));

    const auto p5 = retro::make_unique_small<SmallDerived, const Base>();
    const auto p6 = retro::make_unique_small<const SmallDerived, const Base>();
    ASSERT_TRUE(static_cast<bool>(p5));
    ASSERT_TRUE(static_cast<bool>(p6));

    const auto p7 = retro::make_unique_small<int, int>(1);
    const auto p8 = retro::make_unique_small<int, const int>(1);
    ASSERT_TRUE(static_cast<bool>(p7));
    ASSERT_TRUE(static_cast<bool>(p8));
}

TYPED_TEST(SmallUniquePtrScalarTypedTest, MakeUniqueForOverwriteScalar)
{
    using TestType = TypeParam;

    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small_for_overwrite<TestType>();
            return true;
        }));

    const auto p1 = retro::make_unique_small_for_overwrite<TestType>();
    ASSERT_TRUE(static_cast<bool>(p1));
}

TYPED_TEST(SmallUniquePtrArrayTypedTest, ConstructionArray)
{
    using TestType = TypeParam;

    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<TestType[]>();
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<const TestType[]>();
            return true;
        }));

    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<TestType[]>(nullptr);
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::SmallUniquePtr<const TestType[]>(nullptr);
            return true;
        }));
}

TYPED_TEST(SmallUniquePtrArrayTypedTest, MakeUniqueArray)
{
    using TestType = TypeParam;

    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small<TestType[]>(2);
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small<const TestType[]>(2);
            return true;
        }));

    (void)retro::make_unique_small<TestType[]>(2);
    (void)retro::make_unique_small<const TestType[]>(2);

    (void)retro::make_unique_small<TestType[]>(0);

    SUCCEED();
}

TEST(SmallUniquePtr, MakeUniqueForOverwriteArray)
{
    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small_for_overwrite<SmallPOD[]>(2);
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            (void)retro::make_unique_small_for_overwrite<LargePOD[]>(2);
            return true;
        }));

    (void)retro::make_unique_small_for_overwrite<SmallPOD[]>(2);
    (void)retro::make_unique_small_for_overwrite<LargePOD[]>(2);

    (void)retro::make_unique_small_for_overwrite<SmallPOD[]>(0);
    (void)retro::make_unique_small_for_overwrite<LargePOD[]>(0);

    SUCCEED();
}

TEST(SmallUniquePtr, NoexceptConstruction)
{
    static_assert(noexcept(retro::make_unique_small<SmallDerived>()));
    static_assert(!noexcept(retro::make_unique_small<LargeDerived>()));

    static_assert(noexcept(retro::make_unique_small_for_overwrite<SmallDerived>()));
    static_assert(!noexcept(retro::make_unique_small_for_overwrite<LargeDerived>()));
}

TEST(SmallUniquePtr, IsAlwaysHeapAllocated)
{
    static_assert(!retro::SmallUniquePtr<SmallPOD>::is_always_heap_allocated());
    static_assert(retro::SmallUniquePtr<LargePOD>::is_always_heap_allocated());

    static_assert(!retro::SmallUniquePtr<SmallPOD[]>::is_always_heap_allocated());
    static_assert(retro::SmallUniquePtr<LargePOD[]>::is_always_heap_allocated());

    static_assert(!retro::SmallUniquePtr<SmallDerived>::is_always_heap_allocated());
    static_assert(retro::SmallUniquePtr<LargeDerived>::is_always_heap_allocated());

    static_assert(!retro::SmallUniquePtr<SmallIntrusive>::is_always_heap_allocated());
    static_assert(retro::SmallUniquePtr<LargeIntrusive>::is_always_heap_allocated());
}

TEST(SmallUniquePtr, IsStackAllocated)
{
    static_assert(!std::invoke([] { return retro::make_unique_small<SmallDerived>().is_stack_allocated(); }));
    static_assert(!std::invoke([] { return retro::make_unique_small<LargeDerived>().is_stack_allocated(); }));

    static_assert(!std::invoke([] { return retro::make_unique_small<SmallIntrusive>().is_stack_allocated(); }));
    static_assert(!std::invoke([] { return retro::make_unique_small<LargeIntrusive>().is_stack_allocated(); }));

    static_assert(!std::invoke([] { return retro::make_unique_small<SmallPOD>().is_stack_allocated(); }));
    static_assert(!std::invoke([] { return retro::make_unique_small<LargePOD>().is_stack_allocated(); }));

    static_assert(!std::invoke([] { return retro::make_unique_small<SmallPOD[]>(2).is_stack_allocated(); }));
    static_assert(!std::invoke([] { return retro::make_unique_small<LargePOD[]>(2).is_stack_allocated(); }));

    retro::SmallUniquePtr<Base> p1 = retro::make_unique_small<SmallDerived>();
    retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<LargeDerived>();
    ASSERT_TRUE(p1.is_stack_allocated());
    ASSERT_FALSE(p2.is_stack_allocated());

    retro::SmallUniquePtr<BaseIntrusive> p3 = retro::make_unique_small<SmallIntrusive>();
    retro::SmallUniquePtr<BaseIntrusive> p4 = retro::make_unique_small<LargeIntrusive>();
    ASSERT_TRUE(p3.is_stack_allocated());
    ASSERT_FALSE(p4.is_stack_allocated());

    retro::SmallUniquePtr<SmallPOD> p5 = retro::make_unique_small<SmallPOD>();
    retro::SmallUniquePtr<LargePOD> p6 = retro::make_unique_small<LargePOD>();
    ASSERT_TRUE(p5.is_stack_allocated());
    ASSERT_FALSE(p6.is_stack_allocated());

    retro::SmallUniquePtr<SmallPOD[]> p7 = retro::make_unique_small<SmallPOD[]>(3);
    retro::SmallUniquePtr<LargePOD[]> p8 = retro::make_unique_small<LargePOD[]>(1);
    ASSERT_TRUE(p7.is_stack_allocated());
    ASSERT_FALSE(p8.is_stack_allocated());

    retro::SmallUniquePtr<SmallPOD> np(nullptr);
    ASSERT_FALSE(np.is_stack_allocated());
}

TEST(SmallUniquePtr, Comparisons)
{
    static_assert(retro::SmallUniquePtr<SmallPOD>(nullptr) == nullptr);
    static_assert(retro::SmallUniquePtr<LargePOD>(nullptr) == nullptr);

    static_assert(retro::make_unique_small<SmallDerived>() != nullptr);
    static_assert(retro::make_unique_small<LargeDerived>() != nullptr);

    static_assert(retro::make_unique_small<LargeDerived>() != retro::make_unique_small<Base>());
    static_assert(retro::make_unique_small<Base>() != retro::make_unique_small<SmallDerived>());
}

TYPED_TEST(SmallUniquePtrBoolConvTypedTest, BoolConversion)
{
    using TestType = TypeParam;

    static_assert(!retro::SmallUniquePtr<TestType>());
    static_assert(retro::make_unique_small<TestType>());

    ASSERT_FALSE(static_cast<bool>(retro::SmallUniquePtr<TestType>()));
    ASSERT_TRUE(static_cast<bool>(retro::make_unique_small<TestType>()));
}

TYPED_TEST(SmallUniquePtrGetTypedTest, Get)
{
    using TestType = TypeParam;

    static_assert(retro::SmallUniquePtr<TestType>().get() == nullptr);
    static_assert(retro::make_unique_small<TestType>().get() != nullptr);

    ASSERT_TRUE(retro::SmallUniquePtr<TestType>() == nullptr);
    ASSERT_TRUE(retro::make_unique_small<TestType>().get() != nullptr);
}

TEST(SmallUniquePtr, Dereference)
{
    static_assert((*retro::make_unique_small<SmallDerived>()).padding() == 32);
    static_assert((*retro::make_unique_small<LargeDerived>()).padding() == 64);

    static_assert(retro::make_unique_small<SmallDerived>()->padding() == 32);
    static_assert(retro::make_unique_small<LargeDerived>()->padding() == 64);

    const auto p0 = retro::make_unique_small<const Base>();
    const auto p1 = retro::make_unique_small<SmallDerived>();
    const auto p2 = retro::make_unique_small<LargeDerived>();

    ASSERT_EQ((*p0).value(), 0);
    ASSERT_EQ((*p1).value(), 32);
    ASSERT_EQ((*p2).value(), 64);

    ASSERT_EQ(p0->value(), 0);
    ASSERT_EQ(p1->value(), 32);
    ASSERT_EQ(p2->value(), 64);
}

TEST(SmallUniquePtr, MoveConstructPlain)
{
    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p = retro::make_unique_small<SmallDerived>();
                                return p->padding();
                            }));
    static_assert(64 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p = retro::make_unique_small<LargeDerived>();
                                return p->padding();
                            }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p = retro::make_unique_small<SmallIntrusive>();
                                return p->padding();
                            }));
    static_assert(64 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p = retro::make_unique_small<LargeIntrusive>();
                                return p->padding();
                            }));

    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const SmallPOD> p = retro::make_unique_small<SmallPOD>();
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<volatile LargePOD> p = retro::make_unique_small<LargePOD>();
            return true;
        }));

    retro::SmallUniquePtr<Base> base1 = retro::make_unique_small<SmallDerived>();
    retro::SmallUniquePtr<Base> base2 = retro::make_unique_small<LargeDerived>();
    ASSERT_EQ(base1->value(), 32);
    ASSERT_EQ(base2->value(), 64);

    retro::SmallUniquePtr<const Base> cbase = std::move(base1);
    ASSERT_EQ(cbase->value(), 32);

    retro::SmallUniquePtr<BaseIntrusive> ibase1 = retro::make_unique_small<SmallIntrusive>();
    retro::SmallUniquePtr<BaseIntrusive> ibase2 = retro::make_unique_small<LargeIntrusive>();
    ASSERT_EQ(ibase1->value(), 32);
    ASSERT_EQ(ibase2->value(), 64);

    retro::SmallUniquePtr<const BaseIntrusive> icbase = std::move(ibase1);
    ASSERT_EQ(icbase->value(), 32);

    retro::SmallUniquePtr<const volatile SmallPOD> cpod1 = retro::make_unique_small<SmallPOD>();
    retro::SmallUniquePtr<const volatile LargePOD> cpod2 = retro::make_unique_small<LargePOD>();
    (void)cpod1;
    (void)cpod2;
    SUCCEED();
}

TEST(SmallUniquePtr, MoveConstructArray)
{
    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const SmallPOD[]> p = retro::make_unique_small<SmallPOD[]>(4);
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<volatile LargePOD[]> p = retro::make_unique_small<LargePOD[]>(2);
            return true;
        }));

    retro::SmallUniquePtr<const volatile SmallPOD[]> cpod1 = retro::make_unique_small<SmallPOD[]>(4);
    retro::SmallUniquePtr<const volatile LargePOD[]> cpod2 = retro::make_unique_small<LargePOD[]>(2);

    retro::SmallUniquePtr<SmallPOD[]> cpod3 = retro::make_unique_small<SmallPOD[]>(0);
    retro::SmallUniquePtr<LargePOD[]> cpod4 = retro::make_unique_small<LargePOD[]>(0);

    (void)cpod1;
    (void)cpod2;
    (void)cpod3;
    (void)cpod4;
    SUCCEED();
}

TEST(SmallUniquePtr, MoveAssignmentPlain)
{
    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p;
                                p = retro::make_unique_small<SmallDerived>();
                                return p->padding();
                            }));
    static_assert(64 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p;
                                p = retro::make_unique_small<LargeDerived>();
                                return p->padding();
                            }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p;
                                p = retro::make_unique_small<SmallIntrusive>();
                                return p->padding();
                            }));
    static_assert(64 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p;
                                p = retro::make_unique_small<LargeIntrusive>();
                                return p->padding();
                            }));

    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const SmallPOD> p;
            p = retro::make_unique_small<SmallPOD>();
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const LargePOD> p;
            p = retro::make_unique_small<LargePOD>();
            return true;
        }));

    retro::SmallUniquePtr<const Base> base;

    base = retro::make_unique_small<SmallDerived>();
    ASSERT_EQ(base->value(), 32);

    base = retro::make_unique_small<LargeDerived>();
    ASSERT_EQ(base->value(), 64);

    base = nullptr;
    ASSERT_FALSE(static_cast<bool>(base));

    base.reset(new SmallDerived(1));
    base = retro::make_unique_small<SmallDerived>(2);
    ASSERT_EQ(base->value(), 2);

    retro::SmallUniquePtr<const BaseIntrusive> ibase;

    ibase = retro::make_unique_small<SmallIntrusive>();
    ASSERT_EQ(ibase->value(), 32);

    ibase = retro::make_unique_small<LargeIntrusive>();
    ASSERT_EQ(ibase->value(), 64);

    ibase = nullptr;
    ASSERT_FALSE(static_cast<bool>(ibase));

    ibase.reset(new SmallIntrusive(1));
    ibase = retro::make_unique_small<SmallIntrusive>(2);
    ASSERT_EQ(ibase->value(), 2);

    retro::SmallUniquePtr<const volatile SmallPOD> cpod1;
    cpod1 = retro::make_unique_small<SmallPOD>();
    retro::SmallUniquePtr<const volatile LargePOD> cpod2;
    cpod2 = retro::make_unique_small<LargePOD>();
    (void)cpod1;
    (void)cpod2;
    SUCCEED();
}

TEST(SmallUniquePtr, MoveAssignmentArray)
{
    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const SmallPOD[]> p;
            p = retro::make_unique_small<SmallPOD[]>(4);
            return true;
        }));
    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const LargePOD[]> p;
            p = retro::make_unique_small<LargePOD[]>(4);
            return true;
        }));

    retro::SmallUniquePtr<const volatile SmallPOD[]> cpod1;
    cpod1 = retro::make_unique_small<SmallPOD[]>(4);
    retro::SmallUniquePtr<const volatile LargePOD[]> cpod2;
    cpod2 = retro::make_unique_small<LargePOD[]>(4);
    (void)cpod1;
    (void)cpod2;
    SUCCEED();
}

TEST(SmallUniquePtr, SwapPod)
{
    retro::SmallUniquePtr<const SmallPOD> p1 = nullptr;
    retro::SmallUniquePtr<const SmallPOD> p2 = retro::make_unique_small<const SmallPOD>();

    using std::swap;
    swap(p1, p2);

    ASSERT_TRUE(p2 == nullptr);
    ASSERT_TRUE(p1 != nullptr);
}

TEST(SmallUniquePtr, SwapArray)
{
    retro::SmallUniquePtr<SmallDerived[]> p1 = nullptr;
    retro::SmallUniquePtr<SmallDerived[]> p2 = retro::make_unique_small<SmallDerived[]>(3);

    using std::swap;
    swap(p1, p2);

    ASSERT_TRUE(p2 == nullptr);
    ASSERT_EQ(p1[2].value(), 32);

    swap(p1, p2);

    ASSERT_TRUE(p1 == nullptr);
    ASSERT_EQ(p2[1].value(), 32);
}

TEST(SmallUniquePtr, SwapLarge)
{
    retro::SmallUniquePtr<Base> p1 = nullptr;
    retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<LargeDerived>();

    using std::swap;
    swap(p1, p2);

    ASSERT_TRUE(p2 == nullptr);
    ASSERT_EQ(p1->value(), 64);
}

TEST(SmallUniquePtr, SwapSmall)
{
    retro::SmallUniquePtr<Base> p1 = retro::make_unique_small<SmallDerived>(1);
    retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<SmallDerived>(2);

    ASSERT_EQ(p1->value(), 1);
    ASSERT_EQ(p2->value(), 2);

    using std::swap;
    swap(p1, p2);

    ASSERT_EQ(p1->value(), 2);
    ASSERT_EQ(p2->value(), 1);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 1);
    ASSERT_EQ(p2->value(), 2);
}

TEST(SmallUniquePtr, SwapMixed)
{
    using std::swap;

    retro::SmallUniquePtr<Base> p1 = retro::make_unique_small<SmallDerived>();
    retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<LargeDerived>();

    ASSERT_EQ(p1->value(), 32);
    ASSERT_EQ(p2->value(), 64);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 64);
    ASSERT_EQ(p2->value(), 32);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 32);
    ASSERT_EQ(p2->value(), 64);
}

TEST(SmallUniquePtr, SwapLargeIntrusive)
{
    retro::SmallUniquePtr<BaseIntrusive> p1 = nullptr;
    retro::SmallUniquePtr<BaseIntrusive> p2 = retro::make_unique_small<LargeIntrusive>();

    using std::swap;
    swap(p1, p2);

    ASSERT_TRUE(p2 == nullptr);
    ASSERT_EQ(p1->value(), 64);
}

TEST(SmallUniquePtr, SwapSmallIntrusive)
{
    retro::SmallUniquePtr<BaseIntrusive> p1 = retro::make_unique_small<SmallIntrusive>(1);
    retro::SmallUniquePtr<BaseIntrusive> p2 = retro::make_unique_small<SmallIntrusive>(2);

    using std::swap;
    swap(p1, p2);

    ASSERT_EQ(p1->value(), 2);
    ASSERT_EQ(p2->value(), 1);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 1);
    ASSERT_EQ(p2->value(), 2);
}

TEST(SmallUniquePtr, SwapMixedIntrusive)
{
    using std::swap;

    retro::SmallUniquePtr<BaseIntrusive> p1 = retro::make_unique_small<SmallIntrusive>();
    retro::SmallUniquePtr<BaseIntrusive> p2 = retro::make_unique_small<LargeIntrusive>();

    ASSERT_EQ(p1->value(), 32);
    ASSERT_EQ(p2->value(), 64);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 64);
    ASSERT_EQ(p2->value(), 32);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 32);
    ASSERT_EQ(p2->value(), 64);
}

TEST(SmallUniquePtr, ConstexprSwap)
{
    using std::swap;

    static_assert(std::invoke(
        []
        {
            retro::SmallUniquePtr<const SmallPOD> p1 = retro::make_unique_small<const SmallPOD>();
            retro::SmallUniquePtr<const SmallPOD> p2 = retro::make_unique_small<const SmallPOD>();

            swap(p1, p2);

            return true;
        }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p1 = retro::make_unique_small<SmallDerived>();
                                retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<LargeDerived>();

                                swap(p1, p2);

                                return p2->padding();
                            }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p1 = retro::make_unique_small<SmallDerived>();
                                retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<SmallDerived>();

                                swap(p1, p2);

                                return p2->padding();
                            }));

    static_assert(64 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<Base> p1 = retro::make_unique_small<LargeDerived>();
                                retro::SmallUniquePtr<Base> p2 = retro::make_unique_small<LargeDerived>();

                                swap(p1, p2);

                                return p2->padding();
                            }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p1 = retro::make_unique_small<SmallIntrusive>();
                                retro::SmallUniquePtr<BaseIntrusive> p2 = retro::make_unique_small<LargeIntrusive>();

                                swap(p1, p2);

                                return p2->padding();
                            }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p1 = retro::make_unique_small<SmallIntrusive>();
                                retro::SmallUniquePtr<BaseIntrusive> p2 = retro::make_unique_small<SmallIntrusive>();

                                swap(p1, p2);

                                return p2->padding();
                            }));

    static_assert(64 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<BaseIntrusive> p1 = retro::make_unique_small<LargeIntrusive>();
                                retro::SmallUniquePtr<BaseIntrusive> p2 = retro::make_unique_small<LargeIntrusive>();

                                swap(p1, p2);

                                return p2->padding();
                            }));

    static_assert(32 == std::invoke(
                            []
                            {
                                retro::SmallUniquePtr<SmallDerived[]> p1 = retro::make_unique_small<SmallDerived[]>(2);
                                retro::SmallUniquePtr<SmallDerived[]> p2 = retro::make_unique_small<SmallDerived[]>(4);

                                swap(p1, p2);

                                return p1[3].padding();
                            }));
}

namespace
{
    struct A
    {
        virtual ~A() = default;
    };
    struct C
    {
        virtual ~C() = default;
    };
    struct B
    {
        char b = 2;
        virtual char value() const
        {
            return b;
        }
        virtual ~B() = default;
    };
    struct D : A, B
    {
        char d = 5;
        char value() const override
        {
            return d;
        }
    };
    struct E : C, D
    {
        char e = 6;
        char value() const override
        {
            return e;
        }
    };
} // namespace

TEST(SmallUniquePtr, MoveConstructMultipleInheritance)
{
    retro::SmallUniquePtr<D> derived = retro::make_unique_small<E>();
    ASSERT_EQ(derived->b, 2);
    ASSERT_EQ(derived->value(), 6);

    retro::SmallUniquePtr<B> base = std::move(derived);
    ASSERT_EQ(base->b, 2);
    ASSERT_EQ(base->value(), 6);

    retro::SmallUniquePtr<const B> cbase = std::move(base);
    ASSERT_EQ(cbase->b, 2);
    ASSERT_EQ(cbase->value(), 6);
}

TEST(SmallUniquePtr, MoveAssignmentMultipleInheritance)
{
    retro::SmallUniquePtr<B> p1;

    p1 = retro::make_unique_small<B>();
    ASSERT_EQ(p1->value(), 2);

    p1 = retro::make_unique_small<D>();
    ASSERT_EQ(p1->value(), 5);

    p1 = retro::make_unique_small<B>();
    ASSERT_EQ(p1->value(), 2);

    p1 = retro::make_unique_small<E>();
    ASSERT_EQ(p1->value(), 6);
}

TEST(SmallUniquePtr, SwapMultipleInheritance)
{
    using std::swap;

    retro::SmallUniquePtr<B> p1 = retro::make_unique_small<D>();
    retro::SmallUniquePtr<B> p2 = retro::make_unique_small<E>();

    ASSERT_EQ(p1->value(), 5);
    ASSERT_EQ(p2->value(), 6);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 6);
    ASSERT_EQ(p2->value(), 5);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 5);
    ASSERT_EQ(p2->value(), 6);

    p1 = nullptr;
    swap(p1, p2);

    ASSERT_EQ(p1->value(), 6);
    ASSERT_TRUE(p2 == nullptr);

    swap(p1, p2);

    ASSERT_TRUE(p1 == nullptr);
    ASSERT_EQ(p2->value(), 6);

    swap(p1, p2);

    ASSERT_EQ(p1->value(), 6);
    ASSERT_TRUE(p2 == nullptr);
}

TEST(SmallUniquePtr, VirtualInheritance)
{
    struct VBase
    {
        char a = 1;
        virtual char value() const
        {
            return a;
        }
        virtual ~VBase() = default;
    };
    struct VMiddle1 : virtual VBase
    {
        char value() const override
        {
            return 2;
        }
    };
    struct VMiddle2 : virtual VBase
    {
        char value() const override
        {
            return 3;
        }
    };
    struct VDerived : VMiddle1, VMiddle2
    {
        char d = 4;
        char value() const override
        {
            return d;
        }
    };

    retro::SmallUniquePtr<VMiddle1> middle1 = retro::make_unique_small<VDerived>();
    ASSERT_EQ(middle1->a, 1);
    ASSERT_EQ(middle1->value(), 4);

    retro::SmallUniquePtr<VBase> base1 = std::move(middle1);
    ASSERT_EQ(base1->a, 1);
    ASSERT_EQ(base1->value(), 4);

    retro::SmallUniquePtr<VMiddle1> middle2;
    middle2 = retro::make_unique_small<VDerived>();
    ASSERT_EQ(middle2->a, 1);
    ASSERT_EQ(middle2->value(), 4);

    retro::SmallUniquePtr<VBase> base2 = std::move(middle2);
    ASSERT_EQ(base2->a, 1);
    ASSERT_EQ(base2->value(), 4);
}

TEST(SmallUniquePtr, AbstractBase)
{
    struct ABase
    {
        virtual int value() const = 0;
        virtual ~ABase() = default;
    };
    struct ADerived : ABase
    {
        int value() const override
        {
            return 12;
        }
    };

    retro::SmallUniquePtr<ABase> p = retro::make_unique_small<ADerived>();
    ASSERT_EQ(p->value(), 12);

    retro::SmallUniquePtr<const ABase> pc = std::move(p);
    ASSERT_EQ(pc->value(), 12);
}

TEST(SmallUniquePtr, AlignmentSimple)
{
    retro::SmallUniquePtr<SmallPOD> ps = retro::make_unique_small<SmallPOD>();
    retro::SmallUniquePtr<LargePOD> pl = retro::make_unique_small<LargePOD>();

    ASSERT_EQ((std::bit_cast<std::uintptr_t>(std::addressof(*ps)) % alignof(SmallPOD)), 0u);
    ASSERT_EQ((std::bit_cast<std::uintptr_t>(std::addressof(*pl)) % alignof(LargePOD)), 0u);
}

TEST(SmallUniquePtr, AlignmentArray)
{
    retro::SmallUniquePtr<SmallPOD[]> ps = retro::make_unique_small<SmallPOD[]>(4);
    retro::SmallUniquePtr<LargePOD[]> pl = retro::make_unique_small<LargePOD[]>(2);

    ASSERT_EQ((std::bit_cast<std::uintptr_t>(std::addressof(ps[0])) % alignof(SmallPOD)), 0u);
    ASSERT_EQ((std::bit_cast<std::uintptr_t>(std::addressof(pl[0])) % alignof(LargePOD)), 0u);
}

TEST(SmallUniquePtr, AlignmentPoly)
{
    struct alignas(16) SmallAlign
    {
        virtual ~SmallAlign() = default;
    };
    struct alignas(128) LargeAlign : SmallAlign
    {
    };

    retro::SmallUniquePtr<SmallAlign> p = retro::make_unique_small<LargeAlign>();

    ASSERT_EQ((std::bit_cast<std::uintptr_t>(std::addressof(*p)) % alignof(LargeAlign)), 0u);
}

TEST(SmallUniquePtr, ConstUniquePtr)
{
    const retro::SmallUniquePtr<int> p = retro::make_unique_small<int>(3);
    *p = 2;

    ASSERT_EQ(*p, 2);
}

TEST(SmallUniquePtr, ImmovableObject)
{
    struct IMBase
    {
        virtual int value() const = 0;
        virtual ~IMBase() = default;
    };
    struct IMDerived : IMBase
    {
        int value() const override
        {
            return 7;
        }
        IMDerived() = default;
        IMDerived(IMDerived &&) = delete;
    };

    retro::SmallUniquePtr<IMBase> p1;
    retro::SmallUniquePtr<IMBase> p2 = retro::make_unique_small<IMDerived>();
    ASSERT_EQ(p2->value(), 7);

    p1 = std::move(p2);
    ASSERT_EQ(p1->value(), 7);
}
