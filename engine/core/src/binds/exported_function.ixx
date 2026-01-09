/**
 * @file exported_function.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core:binds.exported_function;

import std;
import :defines;
import :strings.name;

namespace retro
{
    template <typename T>
    struct ArgSizeData
    {
        static constexpr usize value = sizeof(T);
    };

    template <typename T>
    struct ArgSizeData<T &>
    {
        static constexpr usize value = sizeof(T);
    };

    template <typename T>
    constexpr usize ArgSize = ArgSizeData<T>::value;

    template <typename>
    struct FunctionSizeData;

    template <typename ReturnType, typename... Args>
    struct FunctionSizeData<ReturnType (*)(Args...)>
    {
        constexpr static usize value = ArgSize<ReturnType> + (ArgSize<Args> + ... + 0);
    };

    template <typename... Args>
    struct FunctionSizeData<void (*)(Args...)>
    {
        constexpr static usize value = (ArgSize<Args> + ... + 0);
    };

    template <typename T>
    concept SizedFunction = requires(T fn) {
        {
            FunctionSizeData<T>::value
        } -> std::convertible_to<usize>;
    };

    template <SizedFunction F>
    constexpr usize FunctionSize = FunctionSizeData<F>::value;

    export struct ExportedFunction
    {
        Name name;
        void *function_ptr = nullptr;
        usize function_size = 0;

        template <SizedFunction F>
        explicit ExportedFunction(const Name namespace_name, const Name function_name, F function_ptr)
            : ExportedFunction(namespace_name, function_name, function_ptr, FunctionSize<F>)
        {
        }

        RETRO_API ExportedFunction(Name namespace_name, Name function_name, void *function_ptr, usize function_size);
    };
} // namespace retro
