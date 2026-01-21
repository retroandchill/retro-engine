/**
 * @file scripting.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <coreclr_delegates.h>
#include <hostfxr.h>

export module retro.scripting;

import std;
import retro.core;
import retro.runtime;
import boost;

namespace retro
{
    using InitFn = std::remove_pointer_t<hostfxr_initialize_for_runtime_config_fn>;
    using GetDelegateFn = std::remove_pointer_t<hostfxr_get_runtime_delegate_fn>;
    using CloseFn = std::remove_pointer_t<hostfxr_close_fn>;

    export class DotnetLoader;

    export class DotnetInitializationHandle
    {
        // ReSharper disable once CppParameterMayBeConst
        inline explicit DotnetInitializationHandle(hostfxr_handle handle, hostfxr_close_fn close_fptr)
            : handle_{handle}, close_fptr_{close_fptr}
        {
        }

      public:
        inline ~DotnetInitializationHandle()
        {
            close_fptr_(handle_);
        }

        DotnetInitializationHandle(const DotnetInitializationHandle &) = delete;
        DotnetInitializationHandle &operator=(const DotnetInitializationHandle &) = delete;
        DotnetInitializationHandle(DotnetInitializationHandle &&) noexcept = delete;
        DotnetInitializationHandle &operator=(DotnetInitializationHandle &&) noexcept = delete;

        [[nodiscard]] inline hostfxr_handle handle() const noexcept
        {
            return handle_;
        }

      private:
        friend class DotnetLoader;

        hostfxr_handle handle_{};
        hostfxr_close_fn close_fptr_;
    };

    class RETRO_API DotnetLoader
    {
        constexpr static usize MAX_PATH = 260;

      public:
        explicit DotnetLoader();

        [[nodiscard]] DotnetInitializationHandle initialize_for_runtime_config(const std::filesystem::path &path) const;

        template <typename Fn>
            requires std::is_function_v<std::remove_pointer_t<Fn>> && std::is_pointer_v<Fn>
        std::expected<Fn, int32> get_runtime_delegate(const hostfxr_handle handle,
                                                      const hostfxr_delegate_type type) const
        {
            void *function_pointer{nullptr};

            if (const int32 error_code = get_delegate_fptr_(handle, type, &function_pointer); error_code != 0)
            {
                return std::unexpected{error_code};
            }

            return reinterpret_cast<Fn>(function_pointer);
        }

        // ReSharper disable once CppParameterMayBeConst
        inline int32 close(hostfxr_handle handle) const
        {
            return close_fptr_(handle);
        }

      private:
        boost::dll::shared_library lib_;
        hostfxr_initialize_for_runtime_config_fn init_fptr_{nullptr};
        hostfxr_get_runtime_delegate_fn get_delegate_fptr_{nullptr};
        hostfxr_close_fn close_fptr_{nullptr};
    };

    using StartFn = int32(_cdecl *)(const char16_t *assembly_path,
                                    int32 assembly_path_length,
                                    const char16_t *class_name,
                                    int32 class_name_length);
    using TickFn = int32(_cdecl *)(float delta_time, int32 max_tasks);
    using ExitFn = void(_cdecl *)();

    export struct ScriptingCallbacks
    {
        StartFn start = nullptr;
        TickFn tick = nullptr;
        ExitFn exit = nullptr;
    };

    export class RETRO_API DotnetManager final : public ScriptRuntime
    {

      public:
        DotnetManager();

        [[nodiscard]] int32 start_scripts(std::u16string_view assembly_path,
                                          std::u16string_view class_name) const override;

        void tick(float delta_time) override;

        void tear_down() override;

      private:
        [[nodiscard]] load_assembly_and_get_function_pointer_fn initialize_native_host() const;

        DotnetLoader loader_;
        ScriptingCallbacks callbacks_;
    };
} // namespace retro
