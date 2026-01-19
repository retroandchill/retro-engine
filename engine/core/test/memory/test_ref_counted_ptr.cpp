/**
 * @file test_ref_counted_ptr.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "../test_helpers.h"

#include <catch2/catch_test_macros.hpp>

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

TEST_CASE("RefCountPtr: basic construction and destruction", "[RefCountPtr]")
{
    TestObject::reset_counters();
    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 0);

    {
        auto p = make_ref_counted<TestObject>(42);

        REQUIRE(p.get() != nullptr);
        REQUIRE(TestObject::live_count() == 1);
        REQUIRE(p->ref_count() == 1u);
        REQUIRE(p->value() == 42);
    }

    // Last reference left the scope, object must be destroyed
    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 1);
}

TEST_CASE("RefCountPtr: copy construction increments ref count", "[RefCountPtr]")
{
    TestObject::reset_counters();

    RefCountPtr<TestObject> p1;
    {
        p1 = make_ref_counted<TestObject>(10);
        REQUIRE(TestObject::live_count() == 1);
        REQUIRE(p1->ref_count() == 1u);

        RefCountPtr<TestObject> p2{p1};
        REQUIRE(p1.get() == p2.get());
        REQUIRE(p1->ref_count() == 2u);
    }

    // p2 is destroyed, but p1 still keeps the object alive
    REQUIRE(TestObject::live_count() == 1);
    REQUIRE(p1->ref_count() == 1u);

    p1.reset();
    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 1);
}

TEST_CASE("RefCountPtr: copy assignment increments new and releases old", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto p1 = make_ref_counted<TestObject>(1);
    auto p2 = make_ref_counted<TestObject>(2);

    REQUIRE(TestObject::live_count() == 2);
    REQUIRE(p1->ref_count() == 1u);
    REQUIRE(p2->ref_count() == 1u);

    p1 = p2;

    // Old pointee of p1 should be released and destroyed
    REQUIRE(TestObject::live_count() == 1);
    REQUIRE(p1.get() == p2.get());
    REQUIRE(p1->ref_count() == 2u);

    p1.reset();
    REQUIRE(TestObject::live_count() == 1);
    REQUIRE(p2->ref_count() == 1u);

    p2.reset();
    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 2);
}

TEST_CASE("RefCountPtr: move construction transfers pointer and nulls source", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto p1 = make_ref_counted<TestObject>(5);
    REQUIRE(TestObject::live_count() == 1);
    REQUIRE(p1->ref_count() == 1u);

    RefCountPtr<TestObject> p2{std::move(p1)};

    REQUIRE(p1.get() == nullptr);
    REQUIRE(p2.get() != nullptr);
    REQUIRE(p2->ref_count() == 1u);

    p2.reset();
    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 1);
}

TEST_CASE("RefCountPtr: move assignment releases old target and transfers new", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto a = make_ref_counted<TestObject>(1);
    auto b = make_ref_counted<TestObject>(2);

    REQUIRE(TestObject::live_count() == 2);

    auto raw_a = a.get();
    auto raw_b = b.get();

    a = std::move(b);

    // Old a target should be destroyed
    REQUIRE(TestObject::live_count() == 1);
    REQUIRE(a.get() == raw_b);
    REQUIRE(b.get() == nullptr);
    REQUIRE(a->ref_count() == 1u);

    (void)raw_a; // only used to document intent above

    a.reset();
    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 2);
}

TEST_CASE("RefCountPtr: reset() and reset(ptr) semantics", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto p = make_ref_counted<TestObject>(1);
    auto *raw1 = p.get();
    REQUIRE(raw1->ref_count() == 1u);

    SECTION("reset() on non-null releases and nulls")
    {
        p.reset();
        REQUIRE(TestObject::live_count() == 0);
        REQUIRE(TestObject::destruction_count() == 1);
        REQUIRE(p.get() == nullptr);
    }

    SECTION("reset() on null is a no-op")
    {
        p.reset();
        REQUIRE(TestObject::live_count() == 0);
        REQUIRE(TestObject::destruction_count() == 1);

        // This should not crash or change counters
        p.reset();
        REQUIRE(TestObject::live_count() == 0);
        REQUIRE(TestObject::destruction_count() == 1);
    }

    SECTION("reset(ptr) takes ownership and retains")
    {
        p.reset(); // release first created one
        REQUIRE(TestObject::live_count() == 0);
        REQUIRE(TestObject::destruction_count() == 1);

        auto *raw2 = new TestObject{2};
        REQUIRE(TestObject::live_count() == 1);
        REQUIRE(raw2->ref_count() == 0u); // initial refcount

        p.reset(raw2);
        REQUIRE(p.get() == raw2);
        REQUIRE(raw2->ref_count() == 1u); // reset retains

        p.reset();
        REQUIRE(TestObject::live_count() == 0);
        REQUIRE(TestObject::destruction_count() == 2);
    }
}

TEST_CASE("RefCountPtr: reset(ptr) with same pointer is a no-op", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto p = make_ref_counted<TestObject>(3);
    auto *raw = p.get();
    auto old_ref_count = raw->ref_count();

    p.reset(raw);

    REQUIRE(p.get() == raw);
    REQUIRE(raw->ref_count() == old_ref_count);
    REQUIRE(TestObject::live_count() == 1);

    p.reset();
    REQUIRE(TestObject::live_count() == 0);
}

TEST_CASE("RefCountPtr: equality and ordering are pointer-based", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto p1 = make_ref_counted<TestObject>(1);
    RefCountPtr<TestObject> p2{p1};
    auto p3 = make_ref_counted<TestObject>(1);

    REQUIRE(p1 == p2);
    REQUIRE_FALSE(p1 == p3);

    // ordering should follow the underlying pointer addresses
    auto ordering = (p1 <=> p3);
    if (p1.get() < p3.get())
    {
        REQUIRE(ordering == std::strong_ordering::less);
    }
    else if (p1.get() > p3.get())
    {
        REQUIRE(ordering == std::strong_ordering::greater);
    }
    else
    {
        REQUIRE(ordering == std::strong_ordering::equal);
    }
}

TEST_CASE("RefCountPtr: swap exchanges pointees", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto a = make_ref_counted<TestObject>(1);
    auto b = make_ref_counted<TestObject>(2);

    auto *raw_a = a.get();
    auto *raw_b = b.get();

    a.swap(b);
    REQUIRE(a.get() == raw_b);
    REQUIRE(b.get() == raw_a);

    using std::swap;
    swap(a, b);

    REQUIRE(a.get() == raw_a);
    REQUIRE(b.get() == raw_b);
}

TEST_CASE("RefCountPtr: hash uses underlying pointer", "[RefCountPtr]")
{
    TestObject::reset_counters();

    const auto p1 = make_ref_counted<TestObject>(1);
    RefCountPtr p2{p1};
    const auto p3 = make_ref_counted<TestObject>(1);

    const std::hash<RefCountPtr<TestObject>> hasher;

    const auto h1 = hasher(p1);
    const auto h2 = hasher(p2);
    const auto h3 = hasher(p3);

    REQUIRE(h1 == h2);
    REQUIRE(h1 != h3);

    // Also check it behaves as a key in unordered_set
    std::unordered_set<RefCountPtr<TestObject>> set;
    set.insert(p1);
    set.insert(p2); // same pointer, should not increase size
    set.insert(p3);

    REQUIRE(set.size() == 2);
}

TEST_CASE("RefCountPtr: cross-type construction and assignment (base/derived)", "[RefCountPtr]")
{
    TestObject::reset_counters();

    auto d = make_ref_counted<DerivedTestObject>(123);
    REQUIRE(TestObject::live_count() == 1);

    RefCountPtr<TestObject> base{d};
    REQUIRE(base.get() == static_cast<TestObject *>(d.get()));
    REQUIRE(base->ref_count() == 2u);

    RefCountPtr<TestObject> base2;
    base2 = d;
    REQUIRE(base2.get() == base.get());
    REQUIRE(base->ref_count() == 3u);

    base.reset();
    base2.reset();
    d.reset();

    REQUIRE(TestObject::live_count() == 0);
    REQUIRE(TestObject::destruction_count() == 1);
}
