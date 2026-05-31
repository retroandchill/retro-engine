/**
 * @file window.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform.headless.window;

namespace retro
{
    namespace
    {
        std::atomic<std::uint64_t> next_window_id{1};
    }

    HeadlessWindow::HeadlessWindow(const WindowDesc &desc)
        : id_{next_window_id.fetch_add(1, std::memory_order_relaxed)},
          size_{static_cast<std::uint32_t>(desc.width), static_cast<std::uint32_t>(desc.height)}
    {
    }

    HeadlessWindow::HeadlessWindow(NativeWindowHandle) : id_{next_window_id.fetch_add(1, std::memory_order_relaxed)}
    {
    }

    std::uint64_t HeadlessWindow::id() const noexcept
    {
        return id_;
    }

    PlatformWindowHandle HeadlessWindow::platform_handle() const noexcept
    {
        return {.backend = WindowBackend::headless, .handle = nullptr};
    }

    void HeadlessWindow::set_title(CStringView title)
    {
        // Does nothing
    }

    Vector2u HeadlessWindow::size() const
    {
        return size_;
    }
} // namespace retro
