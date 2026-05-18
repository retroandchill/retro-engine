/**
 * @file thread_pool.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.async.thread_pool;

import std;

namespace retro
{
    export class RETRO_API ThreadPool
    {
      public:
        explicit ThreadPool(std::size_t thread_count = 1);

        ThreadPool(const ThreadPool &) = delete;
        ThreadPool(ThreadPool &&) = delete;

        ~ThreadPool();

        ThreadPool &operator=(const ThreadPool &) = delete;
        ThreadPool &operator=(ThreadPool &&) = delete;

        void post(std::packaged_task<void()> task);

        template <std::invocable Functor>
        void post(Functor &&functor)
        {
            post(std::packaged_task<void()>{std::forward<Functor>(functor)});
        }

      private:
        void run() noexcept;

        std::atomic_bool is_active_{true};
        std::vector<std::jthread> threads_;
        std::condition_variable cv_;
        std::mutex guard_;
        std::deque<std::packaged_task<void()>> pending_jobs_;
    };
} // namespace retro
