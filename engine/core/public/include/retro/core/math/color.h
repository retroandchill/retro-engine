/**
 * @file color.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#ifdef __cplusplus
extern "C"
{
#endif

    typedef struct Retro_Color
    {
        float red;
        float green;
        float blue;
        float alpha;
    } Retro_Color;

#ifdef __cplusplus
}
#endif
