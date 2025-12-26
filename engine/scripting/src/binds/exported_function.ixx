//
// Created by fcors on 12/25/2025.
//
module;

#include "retro/core/exports.h"

export module retro.scripting.binds:exported_function;

import std;
import retro.core;

namespace retro {
    template <typename T>
    struct ArgSizeData {
        static constexpr usize value = sizeof(T);
    };

    template <typename T>
    struct ArgSizeData<T&> {
        static constexpr usize value = sizeof(T);
    };

    template <typename T>
    constexpr usize ArgSize = ArgSizeData<T>::value;

    template <typename>
    struct FunctionSizeData;

    template <typename ReturnType, typename... Args>
    struct FunctionSizeData<ReturnType(*)(Args...)> {
        constexpr static usize value = ArgSize<ReturnType> + (ArgSize<Args> + ... + 0);
    };

    template <typename... Args>
    struct FunctionSizeData<void(*)(Args...)> {
        constexpr static usize value = (ArgSize<Args> + ... + 0);
    };

    template <typename T>
    concept SizedFunction = requires(T fn) {
        {FunctionSizeData<T>::value} -> std::convertible_to<usize>;
    };

    template <SizedFunction F>
    constexpr usize FunctionSize = FunctionSizeData<F>::value;

    export struct ExportedFunction {
        Name name;
        void* function_ptr = nullptr;
        usize function_size = 0;

        template <SizedFunction F>
        explicit ExportedFunction(const Name function_name, F function_ptr)  : name(function_name), function_ptr(function_ptr), function_size(FunctionSize<F>) {

        }
    };
}
