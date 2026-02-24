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
import retro.core.math.matrix;
import retro.runtime.world.scene;
import retro.core.util.noncopyable;
import retro.core.containers.optional;
import retro.runtime.rendering.layout.margin;
import retro.runtime.rendering.layout.anchors;
import retro.core.math.rect;
import retro.platform.window;

namespace retro
{
    export class ViewportManager;

    export struct ScreenLayout
    {
        Margin offsets{};
        Anchors anchors{};
        Vector2f alignment{};

        [[nodiscard]] RETRO_API RectI to_screen_rect(Vector2u screen_size) const;

        constexpr friend bool operator==(const ScreenLayout &lhs, const ScreenLayout &rhs) noexcept = default;
    };

    export struct ViewportDrawInfo
    {
        Vector2f viewport_size{};
        Vector2f camera_position{};
        Vector2f camera_pivot{};
        Quaternion2f camera_rotation{};
        float camera_zoom = 1.0f;
    };

    export struct CameraLayout
    {
        Vector2f position{};
        Vector2f pivot{};
        Quaternion2f rotation{};
        float zoom = 1.0f;

        [[nodiscard]] constexpr ViewportDrawInfo get_draw_info(const Vector2u viewport_size) const noexcept
        {
            return ViewportDrawInfo{
                .viewport_size = Vector2f{static_cast<float>(viewport_size.x), static_cast<float>(viewport_size.y)},
                .camera_position = position,
                .camera_pivot = pivot,
                .camera_rotation = rotation,
                .camera_zoom = zoom,
            };
        }
    };

    export class RETRO_API Viewport final
    {
      public:
        using ZOrderChanged = MulticastDelegate<void(Viewport &, std::int32_t)>;
        using WindowChanged =
            MulticastDelegate<void(Viewport &, const std::weak_ptr<Window> &, const std::weak_ptr<Window> &)>;

        inline Viewport(const ScreenLayout &layout, const std::int32_t z_order)
            : screen_layout_{layout}, z_order_{z_order}
        {
        }

        [[nodiscard]] inline const ScreenLayout &screen_layout() const noexcept
        {
            return screen_layout_;
        }

        inline void set_screen_layout(const ScreenLayout &layout) noexcept
        {
            screen_layout_ = layout;
        }

        [[nodiscard]] inline const CameraLayout &camera_layout() const noexcept
        {
            return camera_layout_;
        }

        inline void set_camera_layout(const CameraLayout &camera) noexcept
        {
            camera_layout_ = camera;
        }

        [[nodiscard]] inline std::int32_t z_order() const noexcept
        {
            return z_order_;
        }

        void set_z_order(std::int32_t z_order) noexcept;

        inline ZOrderChanged::RegistrationType on_z_order_changed()
        {
            return ZOrderChanged::RegistrationType{on_z_order_changed_};
        }

        [[nodiscard]] inline Optional<Scene &> scene() const noexcept
        {
            return scene_;
        }

        inline void set_scene(Scene *scene) noexcept
        {
            scene_ = scene;
        }

        [[nodiscard]] inline Optional<std::shared_ptr<Window>> window() const noexcept
        {
            if (auto win = window_.lock(); win != nullptr)
            {
                return std::move(win);
            }

            return std::nullopt;
        }

        void set_window(Window &window) noexcept;

        void clear_window() noexcept;

        inline WindowChanged::RegistrationType on_window_changed()
        {
            return WindowChanged::RegistrationType{on_window_changed_};
        }

      private:
        friend class ViewportManager;

        ScreenLayout screen_layout_;
        CameraLayout camera_layout_;
        std::int32_t z_order_ = 0;
        ZOrderChanged on_z_order_changed_;
        Scene *scene_ = nullptr;
        std::weak_ptr<Window> window_;
        WindowChanged on_window_changed_;
    };

    export using OnViewportDelegate = MulticastDelegate<void(Viewport &)>;

    class RETRO_API ViewportManager final : NonCopyable
    {
      public:
        Viewport &create_viewport(const ScreenLayout &layout = {}, std::int32_t z_order = 0);

        void destroy_viewport(Viewport &viewport);

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
        OnViewportDelegate on_viewport_created_;
        OnViewportDelegate on_viewport_destroyed_;
    };
} // namespace retro
