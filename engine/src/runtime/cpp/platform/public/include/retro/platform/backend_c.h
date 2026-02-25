/**
 * @file backend_c.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif

    typedef uint8_t Retro_PlatformBackendKind;
    enum Retro_PlatformBackendKind_Values
    {
        Retro_PlatformBackendKind_SDL3
    };

    typedef uint32_t Retro_PlatformInitFlags;
    enum Retro_PlatformInitFlags_Values
    {
        Retro_PlatformInitFlags_None = 0,
        Retro_PlatformInitFlags_Audio = 1 << 0,
        Retro_PlatformInitFlags_Video = 1 << 1,
        Retro_PlatformInitFlags_Joystick = 1 << 2,
        Retro_PlatformInitFlags_Haptic = 1 << 3,
        Retro_PlatformInitFlags_Gamepad = 1 << 4,
        Retro_PlatformInitFlags_Events = 1 << 5,
        Retro_PlatformInitFlags_Sensor = 1 << 6,
        Retro_PlatformInitFlags_Camera = 1 << 7
    };

    typedef struct Retro_PlatformBackendInfo
    {
        Retro_PlatformBackendKind kind;
        Retro_PlatformInitFlags flags;
    } RetroPlatformBackendInfo;

    typedef struct Retro_PlatformBackend Retro_PlatformBackend;

    RETRO_API Retro_PlatformBackend *retro_platform_backend_create(RetroPlatformBackendInfo info,
                                                                   char *error_message,
                                                                   int32_t error_message_length);

    RETRO_API void retro_platform_backend_destroy(Retro_PlatformBackend *backend);

#ifdef __cplusplus
}
#endif
