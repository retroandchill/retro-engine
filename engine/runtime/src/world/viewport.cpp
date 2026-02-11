/**
 * @file viewport.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.world.viewport;

namespace retro
{
    RectI ScreenLayout::to_screen_rect(const Vector2u screen_size) const
    {
        const Vector2f clamped_alignment{std::clamp(alignment.x, 0.0f, 1.0f), std::clamp(alignment.y, 0.0f, 1.0f)};

        const float relative_x1 = screen_size.x * anchors.minimum.x + offsets.left;
        const float relative_y1 = screen_size.y * anchors.minimum.y + offsets.top;
        const float relative_x2 = screen_size.x * (1 - anchors.maximum.x) - offsets.right;
        const float relative_y2 = screen_size.y * (1 - anchors.maximum.y) - offsets.bottom;
        const float relative_width = relative_x2 - relative_x1;
        const float relative_height = relative_y2 - relative_y1;
        const float x = relative_x1 - relative_width * clamped_alignment.x;
        const float y = relative_y1 - relative_height * clamped_alignment.y;
        return RectI{.x = static_cast<std::int32_t>(x),
                     .y = static_cast<std::int32_t>(y),
                     .width = static_cast<std::uint32_t>(relative_width),
                     .height = static_cast<std::uint32_t>(relative_height)};
    }

    void Viewport::set_z_order(const std::int32_t z_order) noexcept
    {
        z_order_ = z_order;
        on_z_order_changed_(*this, z_order);
    }

    Viewport &ViewportManager::create_viewport(const ScreenLayout &layout, std::int32_t z_order)
    {
        const auto &new_viewport = viewports_.emplace_back(std::make_unique<Viewport>(layout, z_order));
        on_viewport_created_(*new_viewport);
        new_viewport->on_z_order_changed().add([this](Viewport &, std::int32_t) { sorted_ = false; });
        sorted_ = false;
        return *new_viewport;
    }

    void ViewportManager::destroy_viewport(Viewport &viewport)
    {
        const auto it = std::ranges::find_if(viewports_,
                                             [&viewport](const std::unique_ptr<Viewport> &ptr)
                                             { return ptr.get() == std::addressof(viewport); });

        // We could hypothetically swap and pop here, but often there aren't that many viewports,
        // where moving a few pointers down probably isn't going to waste much time.
        if (it != viewports_.end())
        {
            on_viewport_destroyed_(**it);
            viewports_.erase(it);
        }
    }

    void ViewportManager::sort_by_z_order()
    {
        if (sorted_)
            return;

        std::ranges::sort(viewports_, [](const auto &lhs, const auto &rhs) { return lhs->z_order_ < rhs->z_order_; });
        sorted_ = true;
    }
} // namespace retro
