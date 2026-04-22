/**
 * @file headless_render_backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.headless_render_backend;

import std;
import retro.core.memory.ref_counted_ptr;
import retro.platform.window;
import retro.runtime.rendering.renderer2d;
import retro.runtime.rendering.render_backend;

namespace retro
{
    export class RETRO_API HeadlessRenderBackend final : public RenderBackend
    {
      public:
        std::unique_ptr<Renderer2D> create_renderer(RefCountPtr<Window> window) override;
    };
} // namespace retro
