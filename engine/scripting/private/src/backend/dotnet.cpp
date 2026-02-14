/**
 * @file dotnet.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <coreclr_delegates.h>
#include <nethost.h>

module retro.scripting.backend.dotnet;

import retro.core.strings.cstring_view;

namespace retro
{
    DotnetInitializationHandle DotnetLoader::initialize_for_runtime_config(const std::filesystem::path &path) const
    {
        hostfxr_handle handle;
        if (init_fptr_(path.c_str(), nullptr, &handle) != 0)
        {
            throw std::runtime_error{"hostfxr_initialize_for_runtime_config failed"};
        }

        return DotnetInitializationHandle{handle, close_fptr_};
    }

    DotnetLoader::DotnetLoader()
    {
        std::array<char_t, MAX_PATH> path;
        size_t path_size = std::size(path);
        if (get_hostfxr_path(path.data(), &path_size, nullptr) != 0)
        {
            throw std::runtime_error{"get_hostfxr_path failed to locate hostfxr.dll"};
        }

        const std::filesystem::path dll_path{path.data()};
        if (const auto result = lib_.load(dll_path); !result.has_value())
        {
            throw std::runtime_error{"Failed to LoadLibraryW(hostfxr.dll)"};
        }

        init_fptr_ = lib_.get<InitFn>("hostfxr_initialize_for_runtime_config");
        get_delegate_fptr_ = lib_.get<GetDelegateFn>("hostfxr_get_runtime_delegate");
        close_fptr_ = lib_.get<CloseFn>("hostfxr_close");

        if (init_fptr_ == nullptr || get_delegate_fptr_ == nullptr || close_fptr_ == nullptr)
        {
            lib_.unload();
            throw std::runtime_error{"Failed to resolve hostfxr_initialize_for_runtime_config, "
                                     "hostfxr_get_runtime_delegate, or hostfxr_close"};
        }
    }

    DotnetManager::DotnetManager()
    {
        using InitializeRuntimeHostFn = std::int32_t(_cdecl *)(const char16_t *, std::int32_t, ScriptingCallbacks *);

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
                                                     reinterpret_cast<void **>(&initialize_runtime_host));
            error_code != 0)
        {
            throw std::runtime_error(std::format("Failed to initialize runtime host! Error code: {}", error_code));
        }

        const auto exe_path_u16 = exe_path.u16string();
        if (std::int32_t result_code = initialize_runtime_host(exe_path_u16.data(),
                                                               static_cast<std::int32_t>(exe_path_u16.size()),
                                                               &callbacks_);
            result_code != 0)
        {
            throw std::runtime_error(std::format("Failed to initialize script engine! Error code: {}", result_code));
        }
    }

    std::int32_t DotnetManager::start_scripts(const std::u16string_view assembly_path,
                                              const std::u16string_view class_name) const
    {
        return callbacks_.start(assembly_path.data(),
                                static_cast<std::int32_t>(assembly_path.size()),
                                class_name.data(),
                                static_cast<std::int32_t>(class_name.size()));
    }

    void DotnetManager::tick(const float delta_time)
    {
        callbacks_.tick(delta_time, std::numeric_limits<std::int32_t>::max());
    }

    void DotnetManager::tear_down()
    {
        callbacks_.exit();
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
} // namespace retro
