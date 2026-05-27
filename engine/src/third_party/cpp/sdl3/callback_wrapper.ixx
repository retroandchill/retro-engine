/**
 * @file callback_wrapper.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module sdl:callback_wrapper;

import std;

namespace sdl
{
    template <typename Functor, typename Result, typename... Args>
        requires std::invocable<Functor, Args...> &&
                 std::convertible_to<std::invoke_result_t<Functor, Args...>, Result> && std::move_constructible<Functor>
    struct CallbackWrapper
    {
        using ValueType = Functor;

        CallbackWrapper() = delete;

        static const ValueType &unwrap(void *handle)
        {
            return *static_cast<ValueType *>(handle);
        }

        static Result call(void *handle, Args... args)
        {
            return unwrap(handle)(std::forward<Args>(args)...);
        }

        static Result call_suffixed(Args... args, void *handle)
        {
            return unwrap(handle)(std::forward<Args>(args)...);
        }

        static ValueType *wrap(ValueType &&func)
        {
            return new ValueType{std::move(func)};
        }

        static Result call_once(void *handle, Args... args)
        {
            auto func = release(handle);
            return func(std::forward<Args>(args)...);
        }

        static Result call_once_suffixed(Args... args, void *handle)
        {
            auto func = release(handle);
            return func(std::forward<Args>(args)...);
        }

        static ValueType release(void *handle) noexcept
        {
            if (handle == nullptr)
                return {};

            auto *ptr = static_cast<ValueType *>(handle);
            ValueType func = std::move(*ptr);
            delete ptr;
            return func;
        }
    };
} // namespace sdl
