/**
 * @file test_ref_counted_ptr.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.core;
import std;

using retro::IntrusiveRefCounted;
using retro::make_ref_counted;
using retro::RefCountPtr;

// Simple intrusive ref-counted test object
class TestObject : public IntrusiveRefCounted
{
  public:
    explicit TestObject(const int32 value) noexcept : value_{value}
    {
        ++live_count_;
    }

    ~TestObject() noexcept override
    {
        --live_count_;
        ++destruction_count_;
    }

    [[nodiscard]] int value() const noexcept
    {
        return value_;
    }

    void set_value(int32 v) noexcept
    {
        value_ = v;
    }

    // testing helpers
    [[nodiscard]] static int32 live_count() noexcept
    {
        return live_count_;
    }

    [[nodiscard]] static int32 destruction_count() noexcept
    {
        return destruction_count_;
    }

    static void reset_counters() noexcept
    {
        live_count_ = 0;
        destruction_count_ = 0;
    }

  private:
    int32 value_{};

    inline static int32 live_count_{0};
    inline static int32 destruction_count_{0};
};

// A derived type to test cross-type RefCountPtr operations
class DerivedTestObject : public TestObject
{
  public:
    using TestObject::TestObject;
};

TEST(RefCountPtr, BasicConstructionAndDestruction)
{
    TestObject::reset_counters();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 0);

    {
        auto p = make_ref_counted<TestObject>(42);

        ASSERT_NE(p.get(), nullptr);
        EXPECT_EQ(TestObject::live_count(), 1);
        EXPECT_EQ(p->ref_count(), 1u);
        EXPECT_EQ(p->value(), 42);
    }

    // Last reference left the scope, object must be destroyed
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);
}

TEST(RefCountPtr, CopyConstructionIncrementsRefCount)
{
    TestObject::reset_counters();

    RefCountPtr<TestObject> p1;
    {
        p1 = make_ref_counted<TestObject>(10);
        ASSERT_EQ(TestObject::live_count(), 1);
        ASSERT_EQ(p1->ref_count(), 1u);

        RefCountPtr<TestObject> p2{p1};
        EXPECT_EQ(p1.get(), p2.get());
        EXPECT_EQ(p1->ref_count(), 2u);
    }

    // p2 is destroyed, but p1 still keeps the object alive
    EXPECT_EQ(TestObject::live_count(), 1);
    EXPECT_EQ(p1->ref_count(), 1u);

    p1.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);
}

TEST(RefCountPtr, CopyAssignmentIncrementsNewAndReleasesOld)
{
    TestObject::reset_counters();

    auto p1 = make_ref_counted<TestObject>(1);
    auto p2 = make_ref_counted<TestObject>(2);

    EXPECT_EQ(TestObject::live_count(), 2);
    ASSERT_EQ(p1->ref_count(), 1u);
    ASSERT_EQ(p2->ref_count(), 1u);

    p1 = p2;

    // Old pointee of p1 should be released and destroyed
    EXPECT_EQ(TestObject::live_count(), 1);
    EXPECT_EQ(p1.get(), p2.get());
    EXPECT_EQ(p1->ref_count(), 2u);

    p1.reset();
    EXPECT_EQ(TestObject::live_count(), 1);
    EXPECT_EQ(p2->ref_count(), 1u);

    p2.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 2);
}

TEST(RefCountPtr, MoveConstructionTransfersPointerAndNullsSource)
{
    TestObject::reset_counters();

    auto p1 = make_ref_counted<TestObject>(5);
    EXPECT_EQ(TestObject::live_count(), 1);
    ASSERT_EQ(p1->ref_count(), 1u);

    RefCountPtr<TestObject> p2{std::move(p1)};

    EXPECT_EQ(p1.get(), nullptr);
    ASSERT_NE(p2.get(), nullptr);
    EXPECT_EQ(p2->ref_count(), 1u);

    p2.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);
}

TEST(RefCountPtr, MoveAssignmentReleasesOldTargetAndTransfersNew)
{
    TestObject::reset_counters();

    auto a = make_ref_counted<TestObject>(1);
    auto b = make_ref_counted<TestObject>(2);

    EXPECT_EQ(TestObject::live_count(), 2);

    auto raw_a = a.get();
    auto raw_b = b.get();

    a = std::move(b);

    // Old a target should be destroyed
    EXPECT_EQ(TestObject::live_count(), 1);
    EXPECT_EQ(a.get(), raw_b);
    EXPECT_EQ(b.get(), nullptr);
    EXPECT_EQ(a->ref_count(), 1u);

    (void)raw_a; // only used to document intent above

    a.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 2);
}

TEST(RefCountPtr, ResetOnNonNullReleasesAndNulls)
{
    TestObject::reset_counters();

    auto p = make_ref_counted<TestObject>(1);
    auto *raw1 = p.get();
    ASSERT_EQ(raw1->ref_count(), 1u);

    p.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);
    EXPECT_EQ(p.get(), nullptr);
}

TEST(RefCountPtr, ResetOnNullIsNoOp)
{
    TestObject::reset_counters();

    auto p = make_ref_counted<TestObject>(1);
    p.reset();

    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);

    // This should not crash or change counters
    p.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);
}

TEST(RefCountPtr, ResetWithRawPointerTakesOwnershipAndRetains)
{
    TestObject::reset_counters();

    auto p = make_ref_counted<TestObject>(1);
    p.reset(); // release first created one
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);

    auto *raw2 = new TestObject{2};
    EXPECT_EQ(TestObject::live_count(), 1);
    EXPECT_EQ(raw2->ref_count(), 0u); // initial refcount

    p.reset(raw2);
    EXPECT_EQ(p.get(), raw2);
    EXPECT_EQ(raw2->ref_count(), 1u); // reset retains

    p.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 2);
}

TEST(RefCountPtr, ResetWithSamePointerIsNoOp)
{
    TestObject::reset_counters();

    auto p = make_ref_counted<TestObject>(3);
    auto *raw = p.get();
    auto old_ref_count = raw->ref_count();

    p.reset(raw);

    EXPECT_EQ(p.get(), raw);
    EXPECT_EQ(raw->ref_count(), old_ref_count);
    EXPECT_EQ(TestObject::live_count(), 1);

    p.reset();
    EXPECT_EQ(TestObject::live_count(), 0);
}

TEST(RefCountPtr, EqualityAndOrderingArePointerBased)
{
    TestObject::reset_counters();

    auto p1 = make_ref_counted<TestObject>(1);
    RefCountPtr<TestObject> p2{p1};
    auto p3 = make_ref_counted<TestObject>(1);

    EXPECT_TRUE(p1 == p2);
    EXPECT_FALSE(p1 == p3);

    // ordering should follow the underlying pointer addresses
    auto ordering = (p1 <=> p3);
    if (p1.get() < p3.get())
    {
        EXPECT_EQ(ordering, std::strong_ordering::less);
    }
    else if (p1.get() > p3.get())
    {
        EXPECT_EQ(ordering, std::strong_ordering::greater);
    }
    else
    {
        EXPECT_EQ(ordering, std::strong_ordering::equal);
    }
}

TEST(RefCountPtr, SwapExchangesPointees)
{
    TestObject::reset_counters();

    auto a = make_ref_counted<TestObject>(1);
    auto b = make_ref_counted<TestObject>(2);

    auto *raw_a = a.get();
    auto *raw_b = b.get();

    a.swap(b);
    EXPECT_EQ(a.get(), raw_b);
    EXPECT_EQ(b.get(), raw_a);

    using std::swap;
    swap(a, b);

    EXPECT_EQ(a.get(), raw_a);
    EXPECT_EQ(b.get(), raw_b);
}

TEST(RefCountPtr, HashUsesUnderlyingPointer)
{
    TestObject::reset_counters();

    const auto p1 = make_ref_counted<TestObject>(1);
    RefCountPtr p2{p1};
    const auto p3 = make_ref_counted<TestObject>(1);

    const std::hash<RefCountPtr<TestObject>> hasher;

    const auto h1 = hasher(p1);
    const auto h2 = hasher(p2);
    const auto h3 = hasher(p3);

    EXPECT_EQ(h1, h2);
    EXPECT_NE(h1, h3);

    // Also check it behaves as a key in unordered_set
    std::unordered_set<RefCountPtr<TestObject>> set;
    set.insert(p1);
    set.insert(p2); // same pointer, should not increase size
    set.insert(p3);

    EXPECT_EQ(set.size(), 2u);
}

TEST(RefCountPtr, CrossTypeConstructionAndAssignmentBaseDerived)
{
    TestObject::reset_counters();

    auto d = make_ref_counted<DerivedTestObject>(123);
    EXPECT_EQ(TestObject::live_count(), 1);

    RefCountPtr<TestObject> base{d};
    EXPECT_EQ(base.get(), static_cast<TestObject *>(d.get()));
    EXPECT_EQ(base->ref_count(), 2u);

    RefCountPtr<TestObject> base2;
    base2 = d;
    EXPECT_EQ(base2.get(), base.get());
    EXPECT_EQ(base->ref_count(), 3u);

    base.reset();
    base2.reset();
    d.reset();

    EXPECT_EQ(TestObject::live_count(), 0);
    EXPECT_EQ(TestObject::destruction_count(), 1);
}
