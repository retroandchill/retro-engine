//
// Created by fcors on 12/25/2025.
//
module;

#include "retro/core/exports.h"

export module retro.scripting.binds:exported_function;

import std;
import retro.core;
import retro.core.strings;

namespace retro::scripting {
    using namespace core;

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

    export struct ExportedFunction {
        Name name;
        void* function_ptr = nullptr;
        usize function_size = 0;

        template <SizedFunction F>
        explicit ExportedFunction(const Name module_name, const Name function_name, F function_ptr): ExportedFunction(module_name, function_name, function_ptr, FunctionSizeData<F>::value) {}

    private:
        RETRO_API ExportedFunction(Name module_name, Name function_name, void* function_ptr, usize function_size);
    };
}
