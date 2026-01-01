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
        Scene2D() = default;
        ~Scene2D() = default;

        Scene2D(const Scene2D &) = delete;
        Scene2D(Scene2D &&) = default;
        Scene2D &operator=(const Scene2D &) = delete;
        Scene2D &operator=(Scene2D &&) = default;

        RenderProxyManager &render_proxy_manager()
        {
            return render_proxy_manager_;
        }

        inline Entity &create_entity()
        {
            return entities_.emplace_back();
        }

      private:
        std::vector<Entity> entities_;
        RenderProxyManager render_proxy_manager_;
    };
} // namespace retro