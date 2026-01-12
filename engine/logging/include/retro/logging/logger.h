/**
 * @file logger.h
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
    typedef uint8_t Retro_LogLevel;
    enum
    {
        Retro_LogLevel_Trace,
        Retro_LogLevel_Debug,
        Retro_LogLevel_Info,
        Retro_LogLevel_Warn,
        Retro_LogLevel_Error,
        Retro_LogLevel_Critical,
        Retro_LogLevel_Off
    };

    RETRO_API void retro_log(Retro_LogLevel level, const char16_t *message, int32_t length);

#ifdef __cplusplus
}
#endif
