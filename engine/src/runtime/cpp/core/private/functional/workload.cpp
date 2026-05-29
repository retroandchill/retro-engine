/**
 * @file workload.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.workload;
import retro.core.async.task_actions;

namespace retro
{
    bool Workload::finish(const std::int32_t thread_count) const
    {
        if (chunks_ == 0)
            return true;

        if (thread_count == 1 || chunks_ == 1)
            return finish_sequential();

        if (thread_count > 1)
            return finish_parallel(thread_count).get();

        std::unreachable();
    }

    Task<bool> Workload::finish_async(const std::int32_t thread_count) const
    {
        if (chunks_ == 0)
            return Task<bool>::from_result(true);

        if (thread_count == 1 || chunks_ == 1)
            return Task<bool>::from_result(finish_sequential());

        if (thread_count > 1)
            return finish_parallel(thread_count);

        std::unreachable();
    }

    bool Workload::finish_sequential() const
    {
        for (std::int32_t i = 0; i < chunks_; ++i)
        {
            if (!worker_function_(i, 0))
                return false;
        }

        return true;
    }

    Task<bool> Workload::finish_parallel(const std::int32_t thread_count) const
    {
        std::atomic result{true};
        std::atomic<std::size_t> next{0};
        auto thread_worker = [this, &result, &next](const std::size_t thread_no)
        {
            for (std::size_t i = next++; i < chunks_ && result; i = next++)
            {
                if (!worker_function_(i, thread_no))
                    result = false;
            }
        };

        std::vector<Task<>> tasks;
        tasks.reserve(thread_count);
        for (std::size_t i = 0; i < thread_count; ++i)
            tasks.push_back(run_async(thread_worker, i));
        for (auto &task : tasks)
            co_await std::move(task).configure_await(false);

        co_return result;
    }
} // namespace retro
