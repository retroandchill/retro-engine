/**
 * @file viewport.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.viewport;

import std;
import :scene.rendering;
import :scene.transform;

namespace retro
{
    export class RETRO_API Viewport
    {
      public:
        using IdType = ViewportID;

        explicit inline Viewport(const ViewportID id) : id_{id}
        {
        }
        ~Viewport() = default;

        Viewport(const Viewport &) = delete;
        Viewport(Viewport &&) = default;
        Viewport &operator=(const Viewport &) = delete;
        Viewport &operator=(Viewport &&) = default;

        [[nodiscard]] inline ViewportID id() const noexcept
        {
            return id_;
        }

      private:
        ViewportID id_;
        std::set<RenderObjectID> render_objects_;
    };
} // namespace retro
