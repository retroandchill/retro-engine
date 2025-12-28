//
// Created by fcors on 12/28/2025.
//

export module retro.renderer:render_data;

import retro.core;

namespace retro
{
    struct SolidColorMaterial
    {
        Color color;
    };

    struct TexturedMaterial
    {
        // TODO: Fill me out
    };

    export struct Quad
    {
        Vector2f position;
        Vector2f size;
        Color   color;
    };
}