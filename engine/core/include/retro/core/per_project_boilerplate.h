/**
 * @file per_project_boilerplate.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#define RETRO_VISUALIZER_USE_AND_RETAIN

#define RETRO_VISUALIZER_HELPERS                                                                                       \
    RETRO_VISUALIZER_USE_AND_RETAIN auto *debug_name_table = &retro::debug_get_name_entries();

#define RETRO_PER_PROJECT_BOILERPLATE RETRO_VISUALIZER_HELPERS
