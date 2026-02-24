/**
 * @file services.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.scripting.services;

import retro.core.di;

namespace retro
{
    export RETRO_API void add_scripting_services(ServiceCollection &services);
}
