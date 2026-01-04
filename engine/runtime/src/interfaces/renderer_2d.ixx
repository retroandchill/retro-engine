/**
 * @file renderer_2d.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:interfaces.renderer_2d;

import retro.core;
import std;

namespace retro
{
    export class Renderer2D
    {
      public:
        virtual ~Renderer2D() = default;

        virtual void begin_frame() = 0;

        virtual void end_frame() = 0;

        virtual void queue_draw_calls(Name type, const std::any &data) = 0;
    };
} // namespace retro
