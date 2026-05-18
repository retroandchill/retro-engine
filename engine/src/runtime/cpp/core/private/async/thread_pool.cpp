/**
 * @file thread_pool.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.async.thread_pool;

namespace retro
{

    ThreadPool::ThreadPool(const std::size_t thread_count)
    {
        threads_.reserve(thread_count);
        for (std::size_t i = 0; i < thread_count; ++i)
        {
            threads_.emplace_back(&ThreadPool::run, this);
        }
    }

    ThreadPool::~ThreadPool()
    {
        is_active_ = false;
        cv_.notify_all();
    }

    void ThreadPool::post(std::packaged_task<void()> task)
    {
        std::unique_lock lock{guard_};
        pending_jobs_.emplace_back(std::move(task));
        cv_.notify_one();
    }

    void ThreadPool::run() noexcept
    {
        while (is_active_)
        {
            thread_local std::packaged_task<void()> job;
            {
                std::unique_lock lock{guard_};
                cv_.wait(lock, [&] { return !pending_jobs_.empty() || !is_active_; });
                if (!is_active_)
                    break;
                job.swap(pending_jobs_.front());
                pending_jobs_.pop_front();
            }
            job();
        }
    }
} // namespace retro
