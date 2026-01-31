/**
 * @file test_delegates.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

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

TEST(Delegate, DefaultConstructionAndNullConstruction)
{
    Delegate<int32(int32, int32)> d1;
    Delegate<int32(int32, int32)> d2{nullptr};

    EXPECT_FALSE(d1.is_bound());
    EXPECT_FALSE(d2.is_bound());

    // execute() on unbound should throw
    EXPECT_THROW(d1.execute(1, 2), std::bad_function_call);
    EXPECT_THROW(d2.execute(1, 2), std::bad_function_call);
}

TEST(Delegate, BindToCompileTimeFunction)
{
    auto d = Delegate<int32(int32, int32)>::create<free_function_add>();
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(2, 3), 5);

    // Rebinding to another static function should work
    d.bind<free_function_mul>();
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(2, 3), 6);
}

TEST(Delegate, BindToRuntimeFunction)
{
    auto d = Delegate<int32(int32, int32)>::create(&free_function_add);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(10, 5), 15);

    d.bind(&free_function_mul);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(10, 5), 50);
}

TEST(Delegate, BindToCompileTimeMemberFunction)
{
    TestObject obj;
    obj.factor = 3;
    obj.offset = 4;

    auto d = Delegate<int32(int32)>::create<&TestObject::member_add>(obj);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(5), 3 * 5 + 4);
    EXPECT_EQ(obj.call_count, 1);

    // Non-const member
    d.bind<&TestObject::member_mul>(obj);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(7), 3 * 7);
    EXPECT_EQ(obj.call_count, 2);
}

TEST(Delegate, BindToRuntimeMemberFunction)
{
    TestObject obj;
    obj.factor = 3;
    obj.offset = 4;

    auto d = Delegate<int32(int32)>::create(obj, &TestObject::member_add);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(5), 3 * 5 + 4);
    EXPECT_EQ(obj.call_count, 1);

    // Non-const member
    d.bind(obj, &TestObject::member_mul);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(7), 3 * 7);
    EXPECT_EQ(obj.call_count, 2);
}

TEST(Delegate, BindToLambda)
{
    // Non-capturing lambda – still treated as a functor type here
    auto d = Delegate<int32(int32)>::create([](int32 x) { return x * 2; });
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(4), 8);

    // Capturing lambda (must be stored on heap and deleted on unbind / destruction)
    int32 base = 10;
    d = Delegate<int32(int32)>::create([base](int32 x) { return base + x; });
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(5), 15);
}

TEST(Delegate, BindToFunctorObject)
{
    int32 call_counter = 0;
    auto d = Delegate<int32(int32)>::create(TrackingFunctor{call_counter, 7});
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(3), 10);
    EXPECT_EQ(call_counter, 1);

    // Re-execute to ensure functor object persists while bound
    EXPECT_EQ(d.execute(1), 8);
    EXPECT_EQ(call_counter, 2);
}

TEST(Delegate, ExecuteIfBoundVoid)
{
    Delegate<void(bool &)> d;
    bool flag = false;

    // Unbound -> returns false, does not touch flag
    EXPECT_FALSE(d.execute_if_bound(flag));
    EXPECT_FALSE(flag);

    // Bound
    d.bind<&free_function_void_flag>();
    EXPECT_TRUE(d.is_bound());

    EXPECT_TRUE(d.execute_if_bound(flag));
    EXPECT_TRUE(flag);

    // Unbind and call again
    d.unbind();
    flag = false;
    EXPECT_FALSE(d.execute_if_bound(flag));
    EXPECT_FALSE(flag);
}

TEST(Delegate, ExecuteIfBoundNonVoid)
{
    Delegate<int32(int32, int32)> d;

    // Unbound
    auto res_unbound = d.execute_if_bound(1, 2);
    EXPECT_FALSE(res_unbound.has_value());

    d.bind<free_function_add>();
    auto res_bound = d.execute_if_bound(3, 4);
    EXPECT_TRUE(res_bound.has_value());
    EXPECT_EQ(*res_bound, 7);
}

TEST(Delegate, Unbind)
{
    Delegate<int32(int32, int32)> d;
    d.bind<free_function_add>();

    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(1, 2), 3);

    d.unbind();
    EXPECT_FALSE(d.is_bound());
    EXPECT_THROW(d.execute(1, 2), std::bad_function_call);
}

TEST(Delegate, MoveConstruction)
{
    Delegate<int32(int32, int32)> source;
    source.bind<free_function_mul>();
    ASSERT_TRUE(source.is_bound());
    EXPECT_EQ(source.execute(3, 4), 12);

    Delegate dest{std::move(source)};

    // Source should be empty
    EXPECT_FALSE(source.is_bound());
    EXPECT_THROW(source.execute(2, 2), std::bad_function_call);

    // Destination should work
    ASSERT_TRUE(dest.is_bound());
    EXPECT_EQ(dest.execute(3, 4), 12);
}

TEST(Delegate, MoveAssignment)
{
    Delegate<int32(int32, int32)> d1;
    Delegate<int32(int32, int32)> d2;

    d1.bind<free_function_add>();
    d2.bind<free_function_mul>();

    ASSERT_TRUE(d1.is_bound());
    ASSERT_TRUE(d2.is_bound());
    EXPECT_EQ(d1.execute(1, 2), 3);
    EXPECT_EQ(d2.execute(2, 3), 6);

    d2 = std::move(d1);

    // d1 now empty
    EXPECT_FALSE(d1.is_bound());
    EXPECT_THROW(d1.execute(1, 2), std::bad_function_call);

    // d2 now uses former d1 target
    ASSERT_TRUE(d2.is_bound());
    EXPECT_EQ(d2.execute(1, 2), 3);
}

TEST(Delegate, CopyConstruction)
{
    EXPECT_EQ(CopyTrackingFunctor::instance_count, 0);

    {
        Delegate<int32(int32)> source;
        source.bind(CopyTrackingFunctor{4});

        ASSERT_TRUE(source.is_bound());
        EXPECT_EQ(CopyTrackingFunctor::instance_count, 1);
        EXPECT_EQ(source.execute(3), 7);

        Delegate copy{source};

        ASSERT_TRUE(copy.is_bound());
        EXPECT_EQ(CopyTrackingFunctor::instance_count, 2);
        EXPECT_EQ(copy.execute(3), 7);
        EXPECT_EQ(source.execute(1), 5);
    }

    EXPECT_EQ(CopyTrackingFunctor::instance_count, 0);
}

TEST(Delegate, CopyAssignment)
{
    EXPECT_EQ(LargeTrackingFunctor::instance_count, 0);

    {
        Delegate<int32(int32)> d1;
        Delegate<int32(int32)> d2;

        d1.bind(LargeTrackingFunctor{2});
        d2.bind(LargeTrackingFunctor{7});

        ASSERT_TRUE(d1.is_bound());
        ASSERT_TRUE(d2.is_bound());
        EXPECT_EQ(LargeTrackingFunctor::instance_count, 2);
        EXPECT_EQ(d1.execute(3), 6);
        EXPECT_EQ(d2.execute(3), 21);

        d2 = d1;

        ASSERT_TRUE(d2.is_bound());
        EXPECT_EQ(LargeTrackingFunctor::instance_count, 2);
        EXPECT_EQ(d2.execute(4), 8);
    }

    EXPECT_EQ(LargeTrackingFunctor::instance_count, 0);
}

TEST(Delegate, HeapAllocation)
{
    EXPECT_EQ(DeletionTracker::instance_count, 0);

    {
        Delegate<void()> d;
        d.bind(DeletionTracker{});
        EXPECT_EQ(DeletionTracker::instance_count, 1);
        ASSERT_TRUE(d.is_bound());

        // Move to another delegate to ensure deletion only happens once
        Delegate d2{std::move(d)};
        EXPECT_FALSE(d.is_bound());
        EXPECT_TRUE(d2.is_bound());
        EXPECT_EQ(DeletionTracker::instance_count, 1);
    }

    // After scope exit, both delegates are destroyed and functor should be deleted
    EXPECT_EQ(DeletionTracker::instance_count, 0);
}

TEST(Delegate, WeakBindingSharedPtr)
{
    Delegate<int32(int32)> d;
    auto object = std::make_shared<WeakBindingObject>();

    d.bind(object, &WeakBindingObject::multiply);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(4), 12);

    object.reset();
    EXPECT_FALSE(d.is_bound());
    EXPECT_THROW(d.execute(4), std::bad_function_call);
}

TEST(Delegate, WeakBindingWeakPtr)
{
    Delegate<int32(int32)> d;
    auto object = std::make_shared<WeakBindingObject>();
    std::weak_ptr weak = object;

    d.bind(weak, &WeakBindingObject::multiply);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(2), 6);

    object.reset();
    EXPECT_FALSE(d.is_bound());
    EXPECT_THROW(d.execute(2), std::bad_function_call);
}

TEST(Delegate, WeakBindingEnableSharedFromList)
{
    Delegate<int32(int32)> d;
    auto object = std::make_shared<SharedFromThisObject>();

    d.bind(*object, &SharedFromThisObject::add);
    ASSERT_TRUE(d.is_bound());
    EXPECT_EQ(d.execute(3), 8);

    object.reset();
    EXPECT_FALSE(d.is_bound());
    EXPECT_THROW(d.execute(1), std::bad_function_call);
}

TEST(Delegate, BindWithArgs)
{
    auto d1 = Delegate<int32(int32)>::create<free_function_add>(3);
    ASSERT_TRUE(d1.is_bound());
    EXPECT_EQ(d1.execute(5), 8);

    auto d2 = Delegate<int32(int32)>::create(&free_function_mul, 4);
    ASSERT_TRUE(d2.is_bound());
    EXPECT_EQ(d2.execute(6), 24);

    TestObject obj;
    obj.factor = 2;
    obj.offset = 0;
    auto d3 = Delegate<int32()>::create(obj, &TestObject::member_mul, 7);
    ASSERT_TRUE(d3.is_bound());
    EXPECT_EQ(d3.execute(), 14);

    auto d4 = Delegate<int32(int32)>::create([](int32 x, int32 y) { return x - y; }, 9);
    ASSERT_TRUE(d4.is_bound());
    EXPECT_EQ(d4.execute(20), 11);
}

TEST(Delegate, MulticastReceiver)
{
    MulticastDelegate<void(int32)> multicast;
    int32 total = 0;
    std::vector<int32> values;
    MulticastReceiver receiver;

    auto handle_sum = multicast.add([&](int32 value) { total += value; });
    auto handle_member = multicast.add<&MulticastReceiver::add>(receiver);
    auto handle_append = multicast.add(&append_value, std::ref(values));

    EXPECT_TRUE(handle_sum.is_valid());
    EXPECT_TRUE(handle_member.is_valid());
    EXPECT_TRUE(handle_append.is_valid());
    EXPECT_EQ(multicast.size(), 3);

    multicast.broadcast(4);

    EXPECT_EQ(total, 4);
    EXPECT_EQ(receiver.total, 4);
    EXPECT_EQ(values, std::vector<int32>{4});
}

TEST(Delegate, MulticastRemove)
{
    MulticastDelegate<void(int32)> multicast;
    int32 total = 0;

    const auto handle_first = multicast.add([&](const int32 value) { total += value; });
    multicast.add([&](const int32 value) { total += value * 2; });

    EXPECT_EQ(multicast.size(), 2);

    multicast.remove(handle_first);
    EXPECT_EQ(multicast.size(), 1);

    multicast.broadcast(3);
    EXPECT_EQ(total, 6);
}
