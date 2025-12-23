//
// Created by fcors on 12/23/2025.
//
module;

module retro.scripting.dotnet;

import retro.platform;
import retro.platform.filesystem;

using namespace retro::scripting;
using namespace retro::platform::filesystem;

import std;

DotnetManager::DotnetManager() {
    auto native_host_fptr = initialize_native_host();
}

load_assembly_and_get_function_pointer_fn DotnetManager::initialize_native_host() const {
    const auto runtime_config_path = get_executable_path() / "RetroEngine.runtimeconfig.json";
    const auto init_context = loader_.initialize_for_runtime_config(runtime_config_path);

    auto load_result = loader_.get_runtime_delegate<load_assembly_and_get_function_pointer_fn>(init_context.handle(), hdt_load_assembly_and_get_function_pointer);

    if (!load_result.has_value()) {
        throw new std::runtime_error(std::format("Failed to load assembly and get function pointer! Error code: {}", load_result.error()));
    }

    return load_result.value();
}
