/**
 * @file interop_error.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.interop.interop_error;

import std;
import retro.platform.exceptions;
import retro.core.util.exceptions;
import retro.runtime.exceptions;
import retro.core.async.task;

namespace retro
{
    export enum class InteropErrorCode
    {
        none,
        unknown,
        io_error,
        resource_error,
        invalid_state,
        unsupported_operation,
        not_implemented,
        platform_error,
        graphics_error,
        invalid_argument,
        out_of_range,
        bad_alloc
    };

    export struct InteropError
    {
        InteropErrorCode error_code = InteropErrorCode::none;
        const char *native_exception_type = nullptr;
        const char *message = nullptr;
    };

    template <typename T>
    concept ExpectedLike = requires(T t) {
        {
            t.has_value()
        } -> std::convertible_to<bool>;
        typename T::value_type;
        {
            *t
        } -> std::convertible_to<typename T::value_type>;
        typename T::error_type;
        {
            t.error()
        } -> std::convertible_to<typename T::error_type>;
    };

    const char *cache_error_message(const char *message);

    template <typename T>
    concept ValidInteropResult = std::is_void_v<T> || std::is_lvalue_reference_v<T> || std::is_pointer_v<T>;

    template <typename T>
    concept ValidInteropFunctor = std::invocable<T> && ValidInteropResult<std::invoke_result_t<T>>;

    template <ValidInteropFunctor Functor>
    using InteropResult =
        std::conditional_t<std::is_void_v<std::invoke_result_t<Functor>>,
                           bool,
                           std::conditional_t<std::is_lvalue_reference_v<std::invoke_result_t<Functor>>,
                                              std::remove_reference_t<std::invoke_result_t<Functor>> *,
                                              std::invoke_result_t<Functor>>>;

    template <typename T>
    concept ValidExpectedResult = ExpectedLike<T> && std::is_default_constructible_v<typename T::value_type> &&
                                  std::movable<typename T::value_type>;

    template <typename T>
    concept ValidExpectedFunctor = std::invocable<T> && ValidExpectedResult<std::invoke_result_t<T>>;

    template <ValidExpectedFunctor T>
    using ExpectedInteropResult = std::invoke_result_t<T>::value_type;

    template <ValidExpectedFunctor T>
    using ExpectedErrorType = std::invoke_result_t<T>::error_type;

    template <typename T>
    concept AwaitableExpected = Awaitable<T> && ExpectedLike<AwaitResult<T>>;

    template <typename T>
    concept AwaitableExpectedFunctor = std::invocable<T> && AwaitableExpected<std::invoke_result_t<T>>;

    template <AwaitableExpectedFunctor T>
    using AwaitedExpectedInteropResult = AwaitResult<std::invoke_result_t<T>>::value_type;

    template <AwaitableExpectedFunctor T>
    using AwaitedExpectedInteropError = AwaitResult<std::invoke_result_t<T>>::error_type;

    template <ValidInteropFunctor Functor>
    constexpr InteropResult<Functor> invalid_interop_result = nullptr;

    template <ValidInteropFunctor Functor>
        requires std::is_void_v<InteropResult<Functor>>
    constexpr InteropResult<Functor> invalid_interop_result<Functor> = false;

    export inline InteropErrorCode get_error_code(const std::exception &e)
    {
        if (dynamic_cast<const IoException *>(&e) != nullptr)
        {
            return InteropErrorCode::io_error;
        }

        if (dynamic_cast<const ResourceException *>(&e) != nullptr)
        {
            return InteropErrorCode::resource_error;
        }

        if (dynamic_cast<const InvalidStateException *>(&e) != nullptr)
        {
            return InteropErrorCode::invalid_state;
        }

        if (dynamic_cast<const UnsupportedOperationException *>(&e) != nullptr)
        {
            return InteropErrorCode::unsupported_operation;
        }

        if (dynamic_cast<const NotImplementedException *>(&e) != nullptr)
        {
            return InteropErrorCode::not_implemented;
        }

        if (dynamic_cast<const PlatformException *>(&e) != nullptr)
        {
            return InteropErrorCode::platform_error;
        }

        if (dynamic_cast<const GraphicsException *>(&e) != nullptr)
        {
            return InteropErrorCode::graphics_error;
        }

        if (dynamic_cast<const std::invalid_argument *>(&e) != nullptr)
        {
            return InteropErrorCode::invalid_argument;
        }

        if (dynamic_cast<const std::out_of_range *>(&e) != nullptr)
        {
            return InteropErrorCode::out_of_range;
        }

        if (dynamic_cast<const std::bad_alloc *>(&e) != nullptr)
        {
            return InteropErrorCode::bad_alloc;
        }

        return InteropErrorCode::unknown;
    }

    export template <ValidInteropFunctor Functor>
    InteropResult<Functor> try_execute(Functor &&functor, InteropError &error)
    {
        using ResultType = std::invoke_result_t<Functor>;

        try
        {
            error = InteropError{};
            if constexpr (std::is_void_v<ResultType>)
            {
                std::invoke(std::forward<Functor>(functor));
                return true;
            }
            else if constexpr (std::is_lvalue_reference_v<ResultType>)
            {
                return std::addressof(std::invoke(std::forward<Functor>(functor)));
            }
            else
            {
                auto *result = std::invoke(std::forward<Functor>(functor));
                if (result == nullptr)
                {
                    error.error_code = InteropErrorCode::none;
                    error.native_exception_type = nullptr;
                    error.message = nullptr;
                    return invalid_interop_result<Functor>;
                }

                return result;
            }
        }
        catch (const std::exception &e)
        {
            error.error_code = get_error_code(e);
            auto &type_id = typeid(e);
            error.native_exception_type = type_id.name();
            error.message = cache_error_message(e.what());
            return invalid_interop_result<Functor>;
        }
        catch (...)
        {
            error.error_code = InteropErrorCode::unknown;
            error.native_exception_type = nullptr;
            error.message = "Unknown error";
            return invalid_interop_result<Functor>;
        }
    }

    export template <ValidExpectedFunctor Functor, std::invocable<ExpectedErrorType<Functor>> ErrorHandler>
        requires std::convertible_to<std::invoke_result_t<ErrorHandler, ExpectedErrorType<Functor>>, InteropError>
    bool try_execute(Functor &&functor,
                     ErrorHandler &&error_handle,
                     ExpectedInteropResult<Functor> &success,
                     InteropError &error)
    {
        try
        {
            auto result = std::invoke(std::forward<Functor>(functor));
            if (!result.has_value())
            {
                success = {};
                error = std::invoke(std::forward<ErrorHandler>(error_handle), std::move(result).error());
                return false;
            }

            error = InteropError{};
            success = *std::move(result);
            return true;
        }
        catch (std::exception &e)
        {
            success = {};
            error.error_code = get_error_code(e);
            auto &type_id = typeid(e);
            error.native_exception_type = type_id.name();
            error.message = cache_error_message(e.what());
            return false;
        }
        catch (...)
        {
            success = {};
            error.error_code = InteropErrorCode::unknown;
            error.native_exception_type = nullptr;
            error.message = "Unknown error";
            return false;
        }
    }

    export template <AwaitableExpectedFunctor Functor,
                     std::invocable<AwaitedExpectedInteropError<Functor>> ErrorHandler,
                     std::invocable<AwaitedExpectedInteropResult<Functor>> OnSuccess,
                     std::invocable<InteropError> OnError>
        requires std::convertible_to<std::invoke_result_t<ErrorHandler, AwaitedExpectedInteropError<Functor>>,
                                     InteropError>
    void try_execute_async(Functor &&functor, ErrorHandler &&error_handle, OnSuccess &&on_success, OnError &&on_error)
    {
        std::ignore =
            [](auto local_functor, auto local_error_handle, auto local_on_success, auto local_on_error) -> Task<>
        {
            try
            {
                auto result = co_await std::invoke(std::forward<Functor>(local_functor));
                if (!result.has_value())
                {
                    std::invoke(std::forward<OnError>(local_on_error),
                                std::invoke(std::forward<ErrorHandler>(local_error_handle), std::move(result).error()));
                    co_return;
                }

                std::invoke(std::forward<OnSuccess>(local_on_success), *std::move(result));
            }
            catch (std::exception &e)
            {
                auto &type_id = typeid(e);
                std::invoke(std::forward<OnError>(local_on_error),
                            InteropError{.error_code = get_error_code(e),
                                         .native_exception_type = type_id.name(),
                                         .message = e.what()});
            }
            catch (...)
            {
                std::invoke(std::forward<OnError>(local_on_error),
                            InteropError{.error_code = InteropErrorCode::unknown, .message = "Unknown error"});
            }
        }(std::forward<Functor>(functor),
          std::forward<ErrorHandler>(error_handle),
          std::forward<OnSuccess>(on_success),
          std::forward<OnError>(on_error));
    }
} // namespace retro
