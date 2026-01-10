/**
 * @file dotnet_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <coreclr_delegates.h>

module retro.scripting;

import retro.core;
import retro.runtime;
import retro.scripting;

import std;

using namespace retro;
using namespace retro::filesystem;

DotnetManager::DotnetManager()
{
    using InitializeRuntimeHostFn = int32 (*)(const char16_t *, int32);

    const auto native_host_fptr = initialize_native_host();

    constexpr auto ENTRY_POINT_CLASS_NAME = "RetroEngine.Host.Main, RetroEngine.Host"_nc;
    constexpr auto ENTRY_POINT_METHOD_NAME = "InitializeScriptEngine"_nc;

    const auto exe_path = get_executable_path();
    const auto assembly_path = exe_path / "RetroEngine.Host.dll";

    InitializeRuntimeHostFn initialize_runtime_host{nullptr};

    if (const auto error_code = native_host_fptr(assembly_path.c_str(),
                                                 ENTRY_POINT_CLASS_NAME.data(),
                                                 ENTRY_POINT_METHOD_NAME.data(),
                                                 UNMANAGEDCALLERSONLY_METHOD,
                                                 nullptr,
                                                 std::bit_cast<void **>(&initialize_runtime_host));
        error_code != 0)
    {
        throw std::runtime_error(std::format("Failed to initialize runtime host! Error code: {}", error_code));
    }

    const auto exe_path_u16 = exe_path.u16string();
    if (int32 result_code = initialize_runtime_host(exe_path_u16.data(), static_cast<int32>(exe_path_u16.size()));
        result_code != 0)
    {
        throw std::runtime_error(std::format("Failed to initialize script engine! Error code: {}", result_code));
    }
}

load_assembly_and_get_function_pointer_fn DotnetManager::initialize_native_host() const
{
    const auto runtime_config_path = get_executable_path() / "RetroEngine.runtimeconfig.json";
    const auto init_context = loader_.initialize_for_runtime_config(runtime_config_path);

    auto load_result = loader_.get_runtime_delegate<load_assembly_and_get_function_pointer_fn>(
        init_context.handle(),
        hdt_load_assembly_and_get_function_pointer);

    if (!load_result.has_value())
    {
        throw std::runtime_error(
            std::format("Failed to load assembly and get function pointer! Error code: {}", load_result.error()));
    }

    return load_result.value();
}
