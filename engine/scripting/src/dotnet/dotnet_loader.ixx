/**
 * @file dotnet_loader.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <hostfxr.h>

export module retro.scripting:dotnet.loader;

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
} // namespace retro
