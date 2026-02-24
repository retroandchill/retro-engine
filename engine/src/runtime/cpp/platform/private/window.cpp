/**
 * @file window.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform.window;

namespace retro
{
    namespace
    {
        std::atomic<std::uint64_t> next_window_id{1};
    }

    Window::Window() : id_{next_window_id.fetch_add(1, std::memory_order_relaxed)}
    {
    }
} // namespace retro
