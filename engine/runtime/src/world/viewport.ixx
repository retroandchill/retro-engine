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
import retro.core.functional.delegate;
import retro.core.math.vector;
import retro.runtime.world.scene;
import retro.core.util.noncopyable;
import retro.core.containers.optional;
import retro.runtime.rendering.layout.margin;
import retro.runtime.rendering.layout.anchors;
import retro.core.math.rect;

namespace retro
{
    export class ViewportManager;

    export struct ViewportLayout
    {
        Margin offsets{};
        Anchors anchors{};
        Vector2f alignment{};

        RETRO_API RectI to_screen_rect(Vector2u screen_size) const;

        constexpr friend bool operator==(const ViewportLayout &lhs, const ViewportLayout &rhs) noexcept = default;
    };

    export class RETRO_API Viewport final
    {
      public:
        inline Viewport(const ViewportLayout &layout, const std::int32_t z_order) : layout_{layout}, z_order_{z_order}
        {
        }

        [[nodiscard]] inline const ViewportLayout &layout() const noexcept
        {
            return layout_;
        }

        inline void set_layout(const ViewportLayout &layout) noexcept
        {
            layout_ = layout;
        }

        [[nodiscard]] inline std::int32_t z_order() const noexcept
        {
            return z_order_;
        }

        inline void set_z_order(std::int32_t z_order) noexcept
        {
            z_order_ = z_order;
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

        ViewportLayout layout_;
        std::int32_t z_order_ = 0;
        Scene *scene_ = nullptr;
    };

    export using OnViewportDelegate = MulticastDelegate<void(Viewport &)>;

    class RETRO_API ViewportManager final : NonCopyable
    {
      public:
        Viewport &create_viewport(const ViewportLayout &layout = {}, std::int32_t z_order = 0);

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

        [[nodiscard]] inline OnViewportDelegate::RegistrationType on_viewport_created() noexcept
        {
            return OnViewportDelegate::RegistrationType{on_viewport_created_};
        }

        [[nodiscard]] inline OnViewportDelegate::RegistrationType on_viewport_destroyed() noexcept
        {
            return OnViewportDelegate::RegistrationType{on_viewport_destroyed_};
        }

      private:
        std::vector<std::unique_ptr<Viewport>> viewports_;
        Viewport *primary_ = nullptr;
        OnViewportDelegate on_viewport_created_;
        OnViewportDelegate on_viewport_destroyed_;
    };
} // namespace retro
