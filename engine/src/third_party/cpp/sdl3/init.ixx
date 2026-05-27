/**
 * @file init.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "enum_flags.hpp"

#include <SDL3/SDL_init.h>

export module sdl:init;

import std;
import :error;
import :callback_wrapper;

namespace sdl
{
    export enum class InitFlags : SDL_InitFlags
    {
        none = 0,
        audio = SDL_INIT_AUDIO,
        video = SDL_INIT_VIDEO,
        joystick = SDL_INIT_JOYSTICK,
        haptic = SDL_INIT_HAPTIC,
        gamepad = SDL_INIT_GAMEPAD,
        events = SDL_INIT_EVENTS,
        sensor = SDL_INIT_SENSOR,
        camera = SDL_INIT_CAMERA,
    };

    SDL_DEFINE_ENUM_FLAGS(export, InitFlags)

    export enum class AppResult
    {
        continue_running = SDL_APP_CONTINUE,
        terminate_success = SDL_APP_SUCCESS,
        terminate_failure = SDL_APP_FAILURE
    };

    export using AppInitFunc = SDL_AppIterate_func;
    export using AppIterateFunc = SDL_AppEvent_func;
    export using AppQuitFunc = SDL_AppQuit_func;

    export inline void init(InitFlags flags)
    {
        check_error(SDL_Init(static_cast<SDL_InitFlags>(flags)));
    }

    export inline void init_subsystem(InitFlags flags)
    {
        check_error(SDL_InitSubSystem(static_cast<SDL_InitFlags>(flags)));
    }

    export inline void quit_subsystem(InitFlags flags)
    {
        SDL_QuitSubSystem(static_cast<SDL_InitFlags>(flags));
    }

    export inline bool was_initialized(InitFlags flags)
    {
        return SDL_WasInit(static_cast<SDL_InitFlags>(flags));
    }

    export inline void quit()
    {
        SDL_Quit();
    }

    export inline bool is_main_thread()
    {
        return SDL_IsMainThread();
    }

    export using MainThreadCallback = SDL_MainThreadCallback;

    export inline void run_on_main_thread(const MainThreadCallback callback,
                                          void *user_data,
                                          const bool wait_complete = false)
    {
        check_error(SDL_RunOnMainThread(callback, user_data, wait_complete));
    }

    export template <std::invocable Functor>
    void run_on_main_thread(Functor &&functor, const bool wait_complete = false)
    {
        using Wrapper = CallbackWrapper<std::remove_cvref_t<Functor>, void>;
        auto *wrapped = Wrapper::wrap(std::forward<Functor>(functor));
        run_on_main_thread(&Wrapper::call_once, wrapped, wait_complete);
    }

    export template <StringParam T1, StringParam T2, StringParam T3>
    void set_app_metadata(T1 &&app_name, T2 &&app_version, T3 &&app_identifier)
    {
        check_error(SDL_SetAppMetadata(to_cstring(std::forward<T1>(app_name)),
                                       to_cstring(std::forward<T2>(app_version)),
                                       to_cstring(std::forward<T3>(app_identifier))));
    }

    export template <StringParam T1, StringParam T2>
    void set_app_metadata_property(T1 &&name, T2 &&value)
    {
        check_error(
            SDL_SetAppMetadataProperty(to_cstring(std::forward<T1>(name)), to_cstring(std::forward<T2>(value))));
    }

    export namespace app_properties
    {
        constexpr auto *name = SDL_PROP_APP_METADATA_NAME_STRING;
        constexpr auto *version = SDL_PROP_APP_METADATA_VERSION_STRING;
        constexpr auto *identifier = SDL_PROP_APP_METADATA_IDENTIFIER_STRING;
        constexpr auto *creator = SDL_PROP_APP_METADATA_CREATOR_STRING;
        constexpr auto *copyright = SDL_PROP_APP_METADATA_COPYRIGHT_STRING;
        constexpr auto *url = SDL_PROP_APP_METADATA_URL_STRING;
        constexpr auto *type = SDL_PROP_APP_METADATA_TYPE_STRING;
    } // namespace app_properties

    export template <StringParam T>
    const char *get_app_metadata_property(T &&name)
    {
        return SDL_GetAppMetadataProperty(to_cstring(std::forward<T>(name)));
    }
} // namespace sdl
