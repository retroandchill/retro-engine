/**
 * @file vector.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif

    typedef struct Retro_Vector2i
    {
        int32_t x;
        int32_t y;
    } Retro_Vector2i;

    typedef struct Retro_Vector2u
    {
        uint32_t x;
        uint32_t y;
    } Retro_Vector2u;

    typedef struct Retro_Vector2f
    {
        float x;
        float y;
    } Retro_Vector2f;

    typedef struct Retro_Vector2d
    {
        double x;
        double y;
    } Retro_Vector2d;

    typedef struct Retro_Vector3i
    {
        int32_t x;
        int32_t y;
        int32_t z;
    } Retro_Vector3i;

    typedef struct Retro_Vector3u
    {
        uint32_t x;
        uint32_t y;
        uint32_t z;
    } Retro_Vector3u;

    typedef struct Retro_Vector3f
    {
        float x;
        float y;
        float z;
    } Retro_Vector3f;

    typedef struct Retro_Vector3d
    {
        double x;
        double y;
        double z;
    } Retro_Vector3d;

    typedef struct Retro_Vector4i
    {
        int32_t x;
        int32_t y;
        int32_t z;
        int32_t w;
    } Retro_Vector4i;

    typedef struct Retro_Vector4u
    {
        uint32_t x;
        uint32_t y;
        uint32_t z;
        uint32_t w;
    } Retro_Vector4u;

    typedef struct Retro_Vector4f
    {
        float x;
        float y;
        float z;
        float w;
    } Retro_Vector4f;

    typedef struct Retro_Vector4d
    {
        double x;
        double y;
        double z;
        double w;
    } Retro_Vector4d;

#ifdef __cplusplus
}
#endif
