/**
 * @file viewport.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.world.viewport;

import std;
import retro.core.math.vector;
import retro.runtime.world.scene;
import retro.core.util.noncopyable;
import retro.core.containers.optional;

namespace retro
{
    export class ViewportManager;

    export struct ViewportDescriptor
    {
        Vector2u size{};
    };

    export class RETRO_API Viewport final
    {
      public:
        inline explicit Viewport(const ViewportDescriptor &descriptor) : size_{descriptor.size}
        {
        }

        [[nodiscard]] inline Vector2u size() const noexcept
        {
            return size_;
        }

        inline void set_size(const Vector2u size) noexcept
        {
            size_ = size;
        }

        [[nodiscard]] inline Optional<Scene &> scene() const noexcept
        {
            return scene_;
        }

        inline void set_scene(Scene *scene) noexcept
        {
            scene_ = scene;
        }

      private:
        friend class ViewportManager;

        Vector2u size_;
        Scene *scene_ = nullptr;
    };

    class RETRO_API ViewportManager final : NonCopyable
    {
      public:
        Viewport &create_viewport(const ViewportDescriptor &descriptor = {});

        void destroy_viewport(Viewport &viewport);

        inline void set_primary(Viewport &viewport) noexcept
        {
            primary_ = std::addressof(viewport);
        }

        [[nodiscard]] inline Optional<Viewport &> primary() const noexcept
        {
            return primary_;
        }

        [[nodiscard]] inline std::span<const std::unique_ptr<Viewport>> viewports() const noexcept
        {
            return viewports_;
        }

      private:
        std::vector<std::unique_ptr<Viewport>> viewports_;
        Viewport *primary_ = nullptr;
    };
} // namespace retro
