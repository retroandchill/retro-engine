//
// Created by fcors on 12/29/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.scene2d;

import std;
import retro.core;
import :scene.entity;
import :scene.rendering;

namespace retro
{
    export class RETRO_API Scene2D
    {
      public:
        RenderProxyManager &render_proxy_manager()
        {
            return render_proxy_manager_;
        }

      private:
        std::vector<Entity> entities_;
        RenderProxyManager render_proxy_manager_;
    };
} // namespace retro