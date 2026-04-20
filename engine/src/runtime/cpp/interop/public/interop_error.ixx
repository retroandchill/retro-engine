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
    concept ValidInteropFunctor = std::invocable<T> && (std::is_void_v<std::invoke_result_t<T>> ||
                                                        std::is_lvalue_reference_v<std::invoke_result_t<T>> ||
                                                        std::is_pointer_v<std::invoke_result_t<T>>);

    template <ValidInteropFunctor Functor>
    using InteropResult =
        std::conditional_t<std::is_void_v<std::invoke_result_t<Functor>>,
                           bool,
                           std::conditional_t<std::is_lvalue_reference_v<std::invoke_result_t<Functor>>,
                                              std::remove_reference_t<std::invoke_result_t<Functor>> *,
                                              std::invoke_result_t<Functor>>>;

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
            error.message = e.what();
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
} // namespace retro
