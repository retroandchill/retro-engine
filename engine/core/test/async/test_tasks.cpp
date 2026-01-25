/**
 * @file test_tasks.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <catch2/catch_test_macros.hpp>

import retro.core;
import std;

namespace
{
    // An awaitable that always suspends and schedules the awaiting coroutine onto the current scheduler.
    // This simulates "resume me later on the game thread".
    struct YieldToScheduler
    {
        retro::TaskScheduler *scheduler = nullptr;

        bool await_ready() const noexcept
        {
            return false;
        }

        void await_suspend(std::coroutine_handle<> h) const
        {
            REQUIRE(scheduler != nullptr);
            scheduler->enqueue(h);
        }

        void await_resume() const noexcept
        {
        }
    };

    retro::Task<int> make_ready_value(int v)
    {
        return retro::Task<int>::from_result(v);
    }

    retro::Task<> yield_once_then_set(int &step)
    {
        step = 1;
        co_await YieldToScheduler{retro::TaskScheduler::current()};
        step = 2;
        co_return;
    }
} // namespace

TEST_CASE("Task: awaiting an already-completed task continues inline (no suspension)", "[async][task]")
{
    retro::ManualTaskScheduler scheduler;
    retro::TaskScheduler::Scope scope{&scheduler};

    int step = 0;

    auto parent = [&]() -> retro::Task<>
    {
        step = 1;

        // This task completes immediately; parent should not suspend here.
        const int v = co_await make_ready_value(42);

        REQUIRE(v == 42);
        step = 2;
        co_return;
    }();

    // If await_ready() short-circuits correctly, we should already be done without pumping.
    REQUIRE(step == 2);

    // Pumping should not be required, but also should not break anything.
    REQUIRE(scheduler.pump() == 0);
}

TEST_CASE("Task: yielding to scheduler suspends and requires pump to resume", "[async][task][scheduler]")
{
    retro::ManualTaskScheduler scheduler;
    retro::TaskScheduler::Scope scope{&scheduler};

    int step = 0;

    auto t = yield_once_then_set(step);

    // The coroutine ran until the yield point and suspended.
    REQUIRE(step == 1);

    // One pump should resume it and let it finish.
    REQUIRE(scheduler.pump() == 1);
    REQUIRE(step == 2);

    // Further pumps should have nothing to do.
    REQUIRE(scheduler.pump() == 0);
}

TEST_CASE("Task: child completion resumes parent continuation", "[async][task][final_suspend]")
{
    retro::ManualTaskScheduler scheduler;
    retro::TaskScheduler::Scope scope{&scheduler};

    int step = 0;

    auto child = [&]() -> retro::Task<>
    {
        step = 1;
        co_await YieldToScheduler{retro::TaskScheduler::current()};
        step = 2;
        co_return;
    };

    auto parent = [&]() -> retro::Task<>
    {
        step = 10;
        co_await child();
        step = 20;
        co_return;
    }();

    // Parent starts and then should suspend awaiting child (child yields).
    REQUIRE((step == 1 || step == 10)); // ordering depends on symmetric transfer; both are acceptable here.

    // After one pump we expect:
    // - child resumed and completed (step becomes 2),
    // - and parent continuation resumed (step becomes 20) IF final continuation is resumed inline
    //   while we're in the scheduler context.
    //
    // If your final-await policy enqueues instead, step will be 2 here and will reach 20 on a second pump.
    (void)scheduler.pump();

    REQUIRE((step == 20 || step == 2));

    if (step == 2)
    {
        // Continuation was enqueued; one more pump should resume parent.
        (void)scheduler.pump();
        REQUIRE(step == 20);
    }
}
