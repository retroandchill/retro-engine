/**
 * @file exports.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#ifndef RETRO_STATIC
#if defined(_WIN32) || defined(__CYGWIN__)
#define RETRO_API __declspec(dllexport)
#else
#if __GNUC__ >= 4
#define RETRO_API __attribute__((visibility("default")))
#else
#define RETRO_API
#endif
#endif
#else
#define RETRO_API
#endif
