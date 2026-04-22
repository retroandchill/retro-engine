/**
 * @file render_backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.render_backend;

import std;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;

namespace retro
{
    export class RenderBackend : public IntrusiveRefCounted
    {
      public:
        virtual ~RenderBackend() = default;

        virtual std::unique_ptr<Renderer2D> create_renderer(RefCountPtr<Window> window) = 0;
    };
} // namespace retro
