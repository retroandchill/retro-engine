/**
 * @file viewport.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.world.viewport;

import retro.core.math.operations;

namespace retro
{
    namespace
    {
        constexpr std::pair<float, float> calculate_axis(float parent_size,
                                                         float anchor_min,
                                                         float anchor_max,
                                                         float offset_min,
                                                         float offset_max,
                                                         float alignment)
        {
            alignment = std::clamp(alignment, 0.0f, 1.0f);

            if (nearly_equal(anchor_min, anchor_max))
            {
                const auto size = offset_max;
                const auto anchor_position = parent_size * anchor_min;
                auto position = anchor_position + offset_min - size * alignment;
                return {position, position + size};
            }

            const auto min = parent_size * anchor_min + offset_min;
            const auto max = parent_size * anchor_max - offset_max;
            auto size = max - min;
            auto position = min - size * alignment;
            return {position, size};
        }
    } // namespace

    RectI AnchorData::to_screen_rect(const Vector2u screen_size) const
    {
        auto [x, width] = calculate_axis(screen_size.x,
                                         anchors.minimum.x,
                                         anchors.maximum.x,
                                         offsets.left,
                                         offsets.right,
                                         alignment.x);
        auto [y, height] = calculate_axis(screen_size.y,
                                          anchors.minimum.y,
                                          anchors.maximum.y,
                                          offsets.top,
                                          offsets.bottom,
                                          alignment.y);
        return RectI{.x = static_cast<std::int32_t>(x),
                     .y = static_cast<std::int32_t>(y),
                     .width = static_cast<std::uint32_t>(width),
                     .height = static_cast<std::uint32_t>(height)};
    }

    namespace
    {
        std::uint64_t next_viewport_id = 0;

        std::uint64_t generate_viewport_id() noexcept
        {
            while (next_viewport_id++ == 0)
            {
            }

            return next_viewport_id;
        }
    } // namespace

    Viewport::Viewport(const AnchorData &layout, const std::int32_t z_order)
        : id_{generate_viewport_id()}, screen_layout_{layout}, z_order_{z_order}
    {
    }

    void Viewport::set_screen_layout(const AnchorData &layout) noexcept
    {
        screen_layout_ = layout;
        auto window = window_.lock();
        if (window == nullptr)
            return;

        on_window_resized(window->size());
    }

    RectI Viewport::screen_rect() const noexcept
    {
        const auto window = window_.lock();
        if (window == nullptr)
            return RectI{};

        return screen_layout_.to_screen_rect(window->size());
    }

    void Viewport::set_z_order(const std::int32_t z_order) noexcept
    {
        z_order_ = z_order;
        on_z_order_changed_(*this, z_order);
    }

    void Viewport::set_window(Window &window) noexcept
    {
        const auto old_window = window_;
        window_ = window.weak_from_this();
        if (!old_window.owner_before(window_) && !window_.owner_before(old_window))
            return;
        window_changed_(*this, old_window, window_);
        on_window_resized(window.size());
    }

    void Viewport::clear_window() noexcept
    {
        const auto old_window = window_;
        window_.reset();
        window_changed_(*this, old_window, window_);
    }

    void Viewport::on_window_resized(const Vector2u window_size)
    {
        const auto new_screen_rect = screen_layout_.to_screen_rect(window_size);
        if (new_screen_rect == screen_rect_)
            return;

        screen_rect_ = new_screen_rect;
        screen_rect_changed_.broadcast(screen_rect_);
    }

    ViewportManager::ViewportManager(EventManager &event_manager)
        : event_manager_{event_manager},
          resized_subscription_{event_manager_.window_resized(), *this, &ViewportManager::process_window_resized}
    {
    }

    Viewport &ViewportManager::create_viewport(const AnchorData &layout, std::int32_t z_order)
    {
        const auto &new_viewport = viewports_.emplace_back(std::make_unique<Viewport>(layout, z_order));
        on_viewport_created_(*new_viewport);
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

    void ViewportManager::process_window_resized(const std::uint64_t window_id,
                                                 std::uint32_t width,
                                                 std::uint32_t height) const
    {
        for (auto &viewport : viewports_)
        {
            auto window = viewport->window_.lock();
            if (window == nullptr || window->id() != window_id)
                continue;

            viewport->on_window_resized({width, height});
        }
    }
} // namespace retro
