/**
 * @file future.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#define BOOST_THREAD_PROVIDES_FUTURE
#define BOOST_THREAD_PROVIDES_FUTURE_CONTINUATION

#include <boost/thread/future.hpp>
#include <coroutine>

export module retro.core.async.future;

namespace retro
{
    export template <typename T>
    using Promise = boost::promise<T>;

    export template <typename T>
    using Future = boost::future<T>;

    export template <typename T>
    using SharedFuture = boost::shared_future<T>;

    template <typename T>
    struct FutureAwaiter
    {
        SharedFuture<T> future;

        bool await_ready()
        {
            return future.is_ready();
        }

        void await_suspend(std::coroutine_handle<> handle)
        {
            future.then([handle](auto &&) { handle.resume(); });
        }

        decltype(auto) await_resume()
        {
            return future.get();
        }
    };

    export template <typename T>
    FutureAwaiter<T> operator co_await(Future<T> &&value)
    {
        return FutureAwaiter<T>{value.share()};
    }
} // namespace retro
