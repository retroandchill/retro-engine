/**
 * @file test_ref_counted_ptr.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.core.memory.ref_counted_ptr;
import std;

using retro::IntrusiveRefCounted;
using retro::make_ref_counted;
using retro::RefCountPtr;
using retro::WeakRefCounted;
using retro::WeakRefCountPtr;

// Simple intrusive ref-counted test object
class TestObject : public IntrusiveRefCounted
{
  public:
    explicit TestObject(const std::int32_t value) noexcept : value_{value}
    {
        ++live_count_;
    }

    ~TestObject() noexcept
    {
        --live_count_;
        ++destruction_count_;
    }

    [[nodiscard]] int value() const noexcept
    {
        return value_;
    }

    void set_value(std::int32_t v) noexcept
    {
        value_ = v;
    }

    // testing helpers
    [[nodiscard]] static std::int32_t live_count() noexcept
    {
        return live_count_;
    }

    [[nodiscard]] static std::int32_t destruction_count() noexcept
    {
        return destruction_count_;
    }

    static void reset_counters() noexcept
    {
        live_count_ = 0;
        destruction_count_ = 0;
    }

  private:
    std::int32_t value_{};

    inline static std::int32_t live_count_{0};
    inline static std::int32_t destruction_count_{0};
};

// A derived type to test cross-type RefCountPtr operations
class DerivedTestObject : public TestObject
{
  public:
    using TestObject::TestObject;
};

class WeakTestObject : public WeakRefCounted
{
  public:
    explicit WeakTestObject(const std::int32_t value) noexcept : value_{value}
    {
        ++live_count_;
    }

    ~WeakTestObject() noexcept
    {
        --live_count_;
        ++destruction_count_;
    }

    [[nodiscard]] std::int32_t value() const noexcept
    {
        return value_;
    }

    [[nodiscard]] static std::int32_t live_count() noexcept
    {
        return live_count_;
    }

    [[nodiscard]] static std::int32_t destruction_count() noexcept
    {
        return destruction_count_;
    }

    static void reset_counters() noexcept
    {
        live_count_ = 0;
        destruction_count_ = 0;
    }

  private:
    std::int32_t value_{};

    inline static std::int32_t live_count_{0};
    inline static std::int32_t destruction_count_{0};
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

TEST(WeakRefCountPtr, ConstructionAndWeakCountTracking)
{
    WeakTestObject::reset_counters();

    auto strong = make_ref_counted<WeakTestObject>(77);
    ASSERT_NE(strong.get(), nullptr);
    ASSERT_EQ(WeakTestObject::live_count(), 1);
    ASSERT_EQ(strong->control_block().strong_ref_count(), 1u);
    ASSERT_EQ(strong->control_block().weak_ref_count(), 1u);

    {
        WeakRefCountPtr<WeakTestObject> weak{strong};
        EXPECT_EQ(weak.use_count(), 1u);
        EXPECT_FALSE(weak.expired());
        EXPECT_EQ(strong->control_block().weak_ref_count(), 2u);
    }

    EXPECT_EQ(strong->control_block().weak_ref_count(), 1u);
    strong.reset();
    EXPECT_EQ(WeakTestObject::live_count(), 0);
    EXPECT_EQ(WeakTestObject::destruction_count(), 1);
}

TEST(WeakRefCountPtr, LockPromotesWhileAliveAndExpiresAfterLastStrongRelease)
{
    WeakTestObject::reset_counters();

    auto strong = make_ref_counted<WeakTestObject>(99);
    WeakRefCountPtr<WeakTestObject> weak{strong};
    ASSERT_EQ(strong->ref_count(), 1u);

    {
        auto locked = weak.lock();
        ASSERT_NE(locked.get(), nullptr);
        EXPECT_EQ(locked.get(), strong.get());
        EXPECT_EQ(strong->ref_count(), 2u);
    }

    EXPECT_EQ(strong->ref_count(), 1u);
    strong.reset();
    EXPECT_EQ(WeakTestObject::live_count(), 0);
    EXPECT_EQ(WeakTestObject::destruction_count(), 1);
    EXPECT_TRUE(weak.expired());
    EXPECT_EQ(weak.use_count(), 0u);

    auto locked_after_expire = weak.lock();
    EXPECT_EQ(locked_after_expire.get(), nullptr);
    weak.reset();
}

TEST(WeakRefCountPtr, CopyMoveAndAssignmentMaintainWeakCounts)
{
    WeakTestObject::reset_counters();

    auto strong = make_ref_counted<WeakTestObject>(7);
    auto &control_block = strong->control_block();
    EXPECT_EQ(control_block.weak_ref_count(), 1u);

    WeakRefCountPtr<WeakTestObject> weak1{strong};
    EXPECT_EQ(control_block.weak_ref_count(), 2u);

    WeakRefCountPtr<WeakTestObject> weak2{weak1};
    EXPECT_EQ(control_block.weak_ref_count(), 3u);

    WeakRefCountPtr<WeakTestObject> weak3{std::move(weak2)};
    EXPECT_EQ(control_block.weak_ref_count(), 3u);
    EXPECT_TRUE(weak2.expired());

    WeakRefCountPtr<WeakTestObject> weak4;
    weak4 = weak1;
    EXPECT_EQ(control_block.weak_ref_count(), 4u);

    weak4 = std::move(weak3);
    EXPECT_EQ(control_block.weak_ref_count(), 3u);
    EXPECT_TRUE(weak3.expired());

    strong.reset();
    EXPECT_TRUE(weak1.expired());
    EXPECT_TRUE(weak4.expired());
    EXPECT_EQ(WeakTestObject::live_count(), 0);
    EXPECT_EQ(WeakTestObject::destruction_count(), 1);

    weak1.reset();
    weak4.reset();
}

TEST(WeakRefCountPtr, WeakFromThisMatchesSharedObjectLifetime)
{
    WeakTestObject::reset_counters();

    auto strong = make_ref_counted<WeakTestObject>(11);
    auto weak = strong->weak_from_this();

    EXPECT_FALSE(weak.expired());
    EXPECT_EQ(weak.use_count(), 1u);

    auto locked = weak.lock();
    ASSERT_NE(locked.get(), nullptr);
    EXPECT_EQ(locked->value(), 11);
    locked.reset();

    strong.reset();
    EXPECT_TRUE(weak.expired());
    EXPECT_EQ(weak.lock().get(), nullptr);
    weak.reset();
    EXPECT_EQ(WeakTestObject::destruction_count(), 1);
}
