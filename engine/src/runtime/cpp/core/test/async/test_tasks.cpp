/**
 * @file test_tasks.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>
#include <vulkan/vulkan.hpp>

import retro.core.async.task_scheduler;
import retro.core.async.manual_task_scheduler;
import retro.core.async.task;
import std;
import retro.core.containers.optional;
import retro.core.util.exceptions;

using namespace retro;

namespace
{
    // An awaitable that always suspends and schedules the awaiting coroutine onto the current scheduler.
    // This simulates "resume me later on the game thread".
    struct YieldToScheduler
    {
        Optional<TaskScheduler &> scheduler = std::nullopt;

        bool await_ready() const noexcept
        {
            return false;
        }

        void await_suspend(std::coroutine_handle<> h) const
        {
            ASSERT_TRUE(scheduler.has_value());
            scheduler->enqueue(h);
        }

        void await_resume() const noexcept
        {
        }
    };

    Task<int> make_ready_value(int v)
    {
        return Task<int>::from_result(v);
    }

    Task<> yield_once_then_set(int &step)
    {
        step = 1;
        co_await YieldToScheduler{TaskScheduler::current()};
        step = 2;
        co_return;
    }

    Task<int> cancellable_value(std::stop_token stop_token, int &step)
    {
        step = 1;
        throw_if_stop_requested(stop_token);

        co_await YieldToScheduler{TaskScheduler::current()};

        step = 2;
        throw_if_stop_requested(stop_token);

        co_return 42;
    }

    Task<> cancellable_void(int &step, std::stop_token stop_token)
    {
        step = 1;
        throw_if_stop_requested(stop_token);

        co_await YieldToScheduler{TaskScheduler::current()};

        step = 2;
        throw_if_stop_requested(stop_token);
    }
} // namespace

TEST(Task, AwaitingAlreadyCompletedTaskContinuesInlineNoSuspension)
{
    ManualTaskScheduler scheduler;
    TaskScheduler::Scope scope{&scheduler};

    int step = 0;

    std::ignore = [&]() -> Task<>
    {
        step = 1;

        // This task completes immediately; parent should not suspend here.
        const int v = co_await make_ready_value(42);

        EXPECT_EQ(v, 42);
        step = 2;
        co_return;
    }();

    // If await_ready() short-circuits correctly, we should already be done without pumping.
    EXPECT_EQ(step, 2);

    // Pumping should not be required, but also should not break anything.
    EXPECT_EQ(scheduler.pump(), 0);
}

TEST(Task, YieldingToSchedulerSuspendsAndRequiresPumpToResume)
{
    ManualTaskScheduler scheduler;
    TaskScheduler::Scope scope{&scheduler};

    int step = 0;

    auto t = yield_once_then_set(step);
    (void)t;

    // The coroutine ran until the yield point and suspended.
    EXPECT_EQ(step, 1);

    // One pump should resume it and let it finish.
    EXPECT_EQ(scheduler.pump(), 1);
    EXPECT_EQ(step, 2);

    // Further pumps should have nothing to do.
    EXPECT_EQ(scheduler.pump(), 0);
}

TEST(Task, ChildCompletionResumesParentContinuation)
{
    ManualTaskScheduler scheduler;
    TaskScheduler::Scope scope{&scheduler};

    int step = 0;

    auto child = [&]() -> Task<>
    {
        step = 1;
        co_await YieldToScheduler{TaskScheduler::current()};
        step = 2;
        co_return;
    };

    std::ignore = [&]() -> Task<>
    {
        step = 10;
        co_await child();
        step = 20;
        co_return;
    }();

    // Parent starts and then should suspend awaiting child (child yields).
    EXPECT_TRUE((step == 1 || step == 10)); // ordering depends on symmetric transfer; both are acceptable here.

    // After one pump we expect:
    // - child resumed and completed (step becomes 2),
    // - and parent continuation resumed (step becomes 20) IF final continuation is resumed inline
    //   while we're in the scheduler context.
    //
    // If your final-await policy enqueues instead, step will be 2 here and will reach 20 on a second pump.
    (void)scheduler.pump();

    EXPECT_TRUE((step == 20 || step == 2));

    if (step == 2)
    {
        // Continuation was enqueued; one more pump should resume parent.
        (void)scheduler.pump();
        EXPECT_EQ(step, 20);
    }
}

TEST(Task, StopRequestedBeforeStartCancelsCooperatively)
{
    retro::ManualTaskScheduler scheduler;
    retro::TaskScheduler::Scope scope{&scheduler};

    std::stop_source stop_source;
    stop_source.request_stop();

    int step = 0;
    auto task = cancellable_value(stop_source.get_token(), step);

    EXPECT_EQ(step, 1);
    EXPECT_EQ(scheduler.pump(), 0);

    EXPECT_THROW(std::move(task).get(), retro::OperationCancelledException);
}

TEST(Task, StopRequestedWhileSuspendedCancelsOnResume)
{
    retro::ManualTaskScheduler scheduler;
    retro::TaskScheduler::Scope scope{&scheduler};

    std::stop_source stop_source;

    int step = 0;
    auto task = cancellable_value(stop_source.get_token(), step);

    EXPECT_EQ(step, 1);

    stop_source.request_stop();

    EXPECT_EQ(scheduler.pump(), 1);
    EXPECT_EQ(step, 2);

    EXPECT_THROW(std::move(task).get(), retro::OperationCancelledException);
}

TEST(Task, UnrelatedCancellationExceptionIsObservedAsException)
{
    ManualTaskScheduler scheduler;
    TaskScheduler::Scope scope{&scheduler};

    std::stop_source task_stop_source;
    std::stop_source unrelated_stop_source;
    unrelated_stop_source.request_stop();

    auto task = [](std::stop_token task_stop_token, std::stop_token unrelated_stop_token) -> Task<>
    {
        EXPECT_FALSE(task_stop_token.stop_requested());
        throw OperationCancelledException{unrelated_stop_token};
        co_return;
    }(task_stop_source.get_token(), unrelated_stop_source.get_token());

    EXPECT_THROW(std::move(task).wait(), retro::OperationCancelledException);
}

TEST(Task, AwaitingCancelledChildPropagatesCancellationToParent)
{
    ManualTaskScheduler scheduler;
    TaskScheduler::Scope scope{&scheduler};

    std::stop_source stop_source;

    int step = 0;

    auto parent = [&](std::stop_token stop_token) -> Task<int>
    {
        co_await cancellable_void(step, stop_token);
        co_return 42;
    }(stop_source.get_token());

    EXPECT_EQ(step, 1);

    stop_source.request_stop();

    EXPECT_EQ(scheduler.pump(), 1);
    EXPECT_EQ(step, 2);

    EXPECT_EQ(scheduler.pump(), 1);

    EXPECT_THROW(std::move(parent).get(), retro::OperationCancelledException);
}

TEST(Task, CooperativeCancellationDoesNotRunPastCancellationPoint)
{
    ManualTaskScheduler scheduler;
    TaskScheduler::Scope scope{&scheduler};

    std::stop_source stop_source;

    bool ran_after_cancellation = false;

    auto task = [&](std::stop_token stop_token) -> Task<>
    {
        co_await YieldToScheduler{TaskScheduler::current()};

        throw_if_stop_requested(stop_token);

        ran_after_cancellation = true;
        co_return;
    }(stop_source.get_token());

    stop_source.request_stop();

    EXPECT_EQ(scheduler.pump(), 1);
    EXPECT_FALSE(ran_after_cancellation);
    EXPECT_THROW(std::move(task).wait(), retro::OperationCancelledException);
}
