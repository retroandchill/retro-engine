/**
 * @file headless_render_backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.headless_render_backend;
import retro.runtime.rendering.headless_renderer2d;

namespace retro
{

    std::unique_ptr<Renderer2D> HeadlessRenderBackend::create_renderer(RefCountPtr<Window> window)
    {
        return std::make_unique<HeadlessRenderer2D>(std::move(window));
    }
} // namespace retro
