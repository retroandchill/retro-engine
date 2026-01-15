/**
 * @file engine.h
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

    RETRO_API void retro_engine_request_shutdown(int32_t exit_code);

#ifdef __cplusplus
}
#endif
