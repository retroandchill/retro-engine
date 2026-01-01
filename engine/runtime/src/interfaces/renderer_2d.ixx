//
// Created by fcors on 12/26/2025.
//

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