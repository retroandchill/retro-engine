/**
 * @file test_delegates.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <catch2/catch_test_macros.hpp>

import retro.core;

import std;

using retro::Delegate;
using retro::MulticastDelegate;

namespace
{
    // Helpers for tests
    int32 free_function_add(int32 a, int32 b)
    {
        return a + b;
    }

    int32 free_function_mul(int32 a, int32 b)
    {
        return a * b;
    }

    void free_function_void_flag(bool &flag)
    {
        flag = true;
    }

    void append_value(int32 value, std::vector<int32> &values)
    {
        values.push_back(value);
    }

    struct TestObject
    {
        int32 factor = 2;
        int32 offset = 1;
        mutable int32 call_count = 0;

        [[nodiscard]] int32 member_add(const int32 x) const
        {
            ++call_count;
            return factor * x + offset;
        }

        int32 member_mul(int32 x)
        {
            ++call_count;
            return factor * x;
        }
    };

    struct TrackingFunctor
    {
        int32 &call_counter;
        int32 value{};

        int32 operator()(int32 x) const
        {
            ++call_counter;
            return value + x;
        }
    };

    struct DeletionTracker
    {
        static inline int32 instance_count = 0;

        DeletionTracker()
        {
            ++instance_count;
        }
        DeletionTracker(const DeletionTracker &)
        {
            ++instance_count;
        }
        ~DeletionTracker()
        {
            --instance_count;
        }

        void operator()() const
        { /* no-op */
        }
    };

    struct CopyTrackingFunctor
    {
        static inline int32 instance_count = 0;
        int32 value = 0;

        CopyTrackingFunctor(int32 value = 0) : value(value)
        {
            ++instance_count;
        }
        CopyTrackingFunctor(const CopyTrackingFunctor &other) : value(other.value)
        {
            ++instance_count;
        }
        ~CopyTrackingFunctor()
        {
            --instance_count;
        }

        int32 operator()(int32 x) const
        {
            return value + x;
        }
    };

    struct LargeTrackingFunctor
    {
        static inline int32 instance_count = 0;
        int32 value = 0;
        std::array<char, 64> padding{};

        explicit LargeTrackingFunctor(int32 value = 0) : value(value)
        {
            ++instance_count;
        }
        LargeTrackingFunctor(const LargeTrackingFunctor &other) : value(other.value), padding(other.padding)
        {
            ++instance_count;
        }
        ~LargeTrackingFunctor()
        {
            --instance_count;
        }

        int32 operator()(int32 x) const
        {
            return value * x;
        }
    };

    struct WeakBindingObject
    {
        int32 factor = 3;

        [[nodiscard]] int32 multiply(const int32 x) const
        {
            return factor * x;
        }
    };

    struct SharedFromThisObject : std::enable_shared_from_this<SharedFromThisObject>
    {
        int32 add_base = 5;

        int32 add(int32 x) const
        {
            return add_base + x;
        }
    };

    struct MulticastReceiver
    {
        int32 total = 0;

        void add(int32 value)
        {
            total += value;
        }
    };
} // namespace

TEST_CASE("Delegate default construction and null construction", "[Delegate]")
{
    Delegate<int32(int32, int32)> d1;
    Delegate<int32(int32, int32)> d2{nullptr};

    REQUIRE_FALSE(d1.is_bound());
    REQUIRE_FALSE(d2.is_bound());

    // execute() on unbound should throw
    REQUIRE_THROWS_AS(d1.execute(1, 2), std::bad_function_call);
    REQUIRE_THROWS_AS(d2.execute(1, 2), std::bad_function_call);
}

TEST_CASE("Delegate bind_static compile-time function pointer", "[Delegate]")
{
    auto d = Delegate<int32(int32, int32)>::create<free_function_add>();
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(2, 3) == 5);

    // Rebinding to another static function should work
    d.bind<free_function_mul>();
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(2, 3) == 6);
}

TEST_CASE("Delegate bind_static runtime function pointer", "[Delegate]")
{
    auto d = Delegate<int32(int32, int32)>::create(&free_function_add);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(10, 5) == 15);

    d.bind(&free_function_mul);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(10, 5) == 50);
}

TEST_CASE("Delegate bind_raw with compile-time member pointer", "[Delegate]")
{
    TestObject obj;
    obj.factor = 3;
    obj.offset = 4;

    auto d = Delegate<int32(int32)>::create<&TestObject::member_add>(obj);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(5) == 3 * 5 + 4);
    REQUIRE(obj.call_count == 1);

    // Non-const member
    d.bind<&TestObject::member_mul>(obj);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(7) == 3 * 7);
    REQUIRE(obj.call_count == 2);
}

TEST_CASE("Delegate bind_raw with runtime member type parameter", "[Delegate]")
{
    TestObject obj;
    obj.factor = 3;
    obj.offset = 4;

    auto d = Delegate<int32(int32)>::create(obj, &TestObject::member_add);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(5) == 3 * 5 + 4);
    REQUIRE(obj.call_count == 1);

    // Non-const member
    d.bind(obj, &TestObject::member_mul);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(7) == 3 * 7);
    REQUIRE(obj.call_count == 2);
}

TEST_CASE("Delegate bind_lambda with non-capturing and capturing lambdas", "[Delegate]")
{
    // Non-capturing lambda – still treated as a functor type here
    auto d = Delegate<int32(int32)>::create([](int32 x) { return x * 2; });
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(4) == 8);

    // Capturing lambda (must be stored on heap and deleted on unbind / destruction)
    int32 base = 10;
    d = Delegate<int32(int32)>::create([base](int32 x) { return base + x; });
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(5) == 15);
}

TEST_CASE("Delegate bind_lambda with stateful functor", "[Delegate]")
{
    int32 call_counter = 0;
    auto d = Delegate<int32(int32)>::create(TrackingFunctor{call_counter, 7});
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(3) == 10);
    REQUIRE(call_counter == 1);

    // Re-execute to ensure functor object persists while bound
    REQUIRE(d.execute(1) == 8);
    REQUIRE(call_counter == 2);
}

TEST_CASE("Delegate execute_if_bound for void return type", "[Delegate]")
{
    Delegate<void(bool &)> d;
    bool flag = false;

    // Unbound -> returns false, does not touch flag
    REQUIRE_FALSE(d.execute_if_bound(flag));
    REQUIRE_FALSE(flag);

    // Bound
    d.bind<&free_function_void_flag>();
    REQUIRE(d.is_bound());

    REQUIRE(d.execute_if_bound(flag));
    REQUIRE(flag);

    // Unbind and call again
    d.unbind();
    flag = false;
    REQUIRE_FALSE(d.execute_if_bound(flag));
    REQUIRE_FALSE(flag);
}

TEST_CASE("Delegate execute_if_bound for non-void return type", "[Delegate]")
{
    Delegate<int32(int32, int32)> d;

    // Unbound
    auto res_unbound = d.execute_if_bound(1, 2);
    REQUIRE_FALSE(res_unbound.has_value());

    d.bind<free_function_add>();
    auto res_bound = d.execute_if_bound(3, 4);
    REQUIRE(res_bound.has_value());
    REQUIRE(*res_bound == 7);
}

TEST_CASE("Delegate unbind releases state and makes delegate unusable", "[Delegate]")
{
    Delegate<int32(int32, int32)> d;
    d.bind<free_function_add>();

    REQUIRE(d.is_bound());
    REQUIRE(d.execute(1, 2) == 3);

    d.unbind();
    REQUIRE_FALSE(d.is_bound());
    REQUIRE_THROWS_AS(d.execute(1, 2), std::bad_function_call);
}

TEST_CASE("Delegate move construction transfers binding", "[Delegate]")
{
    Delegate<int32(int32, int32)> source;
    source.bind<free_function_mul>();
    REQUIRE(source.is_bound());
    REQUIRE(source.execute(3, 4) == 12);

    Delegate dest{std::move(source)};

    // Source should be empty
    REQUIRE_FALSE(source.is_bound());
    REQUIRE_THROWS_AS(source.execute(2, 2), std::bad_function_call);

    // Destination should work
    REQUIRE(dest.is_bound());
    REQUIRE(dest.execute(3, 4) == 12);
}

TEST_CASE("Delegate move assignment transfers binding and cleans up previous", "[Delegate]")
{
    Delegate<int32(int32, int32)> d1;
    Delegate<int32(int32, int32)> d2;

    d1.bind<free_function_add>();
    d2.bind<free_function_mul>();

    REQUIRE(d1.is_bound());
    REQUIRE(d2.is_bound());
    REQUIRE(d1.execute(1, 2) == 3);
    REQUIRE(d2.execute(2, 3) == 6);

    d2 = std::move(d1);

    // d1 now empty
    REQUIRE_FALSE(d1.is_bound());
    REQUIRE_THROWS_AS(d1.execute(1, 2), std::bad_function_call);

    // d2 now uses former d1 target
    REQUIRE(d2.is_bound());
    REQUIRE(d2.execute(1, 2) == 3);
}

TEST_CASE("Delegate copy construction clones bound functor", "[Delegate]")
{
    REQUIRE(CopyTrackingFunctor::instance_count == 0);

    {
        Delegate<int32(int32)> source;
        source.bind(CopyTrackingFunctor{4});

        REQUIRE(source.is_bound());
        REQUIRE(CopyTrackingFunctor::instance_count == 1);
        REQUIRE(source.execute(3) == 7);

        Delegate copy{source};

        REQUIRE(copy.is_bound());
        REQUIRE(CopyTrackingFunctor::instance_count == 2);
        REQUIRE(copy.execute(3) == 7);
        REQUIRE(source.execute(1) == 5);
    }

    REQUIRE(CopyTrackingFunctor::instance_count == 0);
}

TEST_CASE("Delegate copy assignment replaces existing binding", "[Delegate]")
{
    REQUIRE(LargeTrackingFunctor::instance_count == 0);

    {
        Delegate<int32(int32)> d1;
        Delegate<int32(int32)> d2;

        d1.bind(LargeTrackingFunctor{2});
        d2.bind(LargeTrackingFunctor{7});

        REQUIRE(d1.is_bound());
        REQUIRE(d2.is_bound());
        REQUIRE(LargeTrackingFunctor::instance_count == 2);
        REQUIRE(d1.execute(3) == 6);
        REQUIRE(d2.execute(3) == 21);

        d2 = d1;

        REQUIRE(d2.is_bound());
        REQUIRE(LargeTrackingFunctor::instance_count == 2);
        REQUIRE(d2.execute(4) == 8);
    }

    REQUIRE(LargeTrackingFunctor::instance_count == 0);
}

TEST_CASE("Delegate properly deletes heap-stored functor on destruction", "[Delegate]")
{
    REQUIRE(DeletionTracker::instance_count == 0);

    {
        Delegate<void()> d;
        d.bind(DeletionTracker{});
        REQUIRE(DeletionTracker::instance_count == 1);
        REQUIRE(d.is_bound());

        // Move to another delegate to ensure deletion only happens once
        Delegate d2{std::move(d)};
        REQUIRE_FALSE(d.is_bound());
        REQUIRE(d2.is_bound());
        REQUIRE(DeletionTracker::instance_count == 1);
    }

    // After scope exit, both delegates are destroyed and functor should be deleted
    REQUIRE(DeletionTracker::instance_count == 0);
}

TEST_CASE("Delegate weak binding with std::shared_ptr", "[Delegate]")
{
    Delegate<int32(int32)> d;
    auto object = std::make_shared<WeakBindingObject>();

    d.bind(object, &WeakBindingObject::multiply);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(4) == 12);

    object.reset();
    REQUIRE_FALSE(d.is_bound());
    REQUIRE_THROWS_AS(d.execute(4), std::bad_function_call);
}

TEST_CASE("Delegate weak binding with std::weak_ptr", "[Delegate]")
{
    Delegate<int32(int32)> d;
    auto object = std::make_shared<WeakBindingObject>();
    std::weak_ptr weak = object;

    d.bind(weak, &WeakBindingObject::multiply);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(2) == 6);

    object.reset();
    REQUIRE_FALSE(d.is_bound());
    REQUIRE_THROWS_AS(d.execute(2), std::bad_function_call);
}

TEST_CASE("Delegate weak binding with enable_shared_from_this", "[Delegate]")
{
    Delegate<int32(int32)> d;
    auto object = std::make_shared<SharedFromThisObject>();

    d.bind(*object, &SharedFromThisObject::add);
    REQUIRE(d.is_bound());
    REQUIRE(d.execute(3) == 8);

    object.reset();
    REQUIRE_FALSE(d.is_bound());
    REQUIRE_THROWS_AS(d.execute(1), std::bad_function_call);
}

TEST_CASE("Delegate bind with additional arguments", "[Delegate]")
{
    auto d1 = Delegate<int32(int32)>::create<free_function_add>(3);
    REQUIRE(d1.is_bound());
    REQUIRE(d1.execute(5) == 8);

    auto d2 = Delegate<int32(int32)>::create(&free_function_mul, 4);
    REQUIRE(d2.is_bound());
    REQUIRE(d2.execute(6) == 24);

    TestObject obj;
    obj.factor = 2;
    obj.offset = 0;
    auto d3 = Delegate<int32()>::create(obj, &TestObject::member_mul, 7);
    REQUIRE(d3.is_bound());
    REQUIRE(d3.execute() == 14);

    auto d4 = Delegate<int32(int32)>::create([](int32 x, int32 y) { return x - y; }, 9);
    REQUIRE(d4.is_bound());
    REQUIRE(d4.execute(20) == 11);
}

TEST_CASE("MulticastDelegate add and broadcast with different bindings", "[Delegate]")
{
    MulticastDelegate<void(int32)> multicast;
    int32 total = 0;
    std::vector<int32> values;
    MulticastReceiver receiver;

    auto handle_sum = multicast.add([&](int32 value) { total += value; });
    auto handle_member = multicast.add<&MulticastReceiver::add>(receiver);
    auto handle_append = multicast.add(&append_value, std::ref(values));

    REQUIRE(handle_sum.is_valid());
    REQUIRE(handle_member.is_valid());
    REQUIRE(handle_append.is_valid());
    REQUIRE(multicast.size() == 3);

    multicast.broadcast(4);

    REQUIRE(total == 4);
    REQUIRE(receiver.total == 4);
    REQUIRE(values == std::vector<int32>{4});
}

TEST_CASE("MulticastDelegate remove stops future calls", "[Delegate]")
{
    MulticastDelegate<void(int32)> multicast;
    int32 total = 0;

    const auto handle_first = multicast.add([&](const int32 value) { total += value; });
    multicast.add([&](const int32 value) { total += value * 2; });

    REQUIRE(multicast.size() == 2);

    multicast.remove(handle_first);
    REQUIRE(multicast.size() == 1);

    multicast.broadcast(3);
    REQUIRE(total == 6);
}
