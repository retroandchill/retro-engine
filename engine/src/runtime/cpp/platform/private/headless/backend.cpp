/**
 * @file backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform.headless.backend;

import retro.platform.headless.window;

namespace retro
{

    WindowBackend HeadlessPlatformBackend::window_backend() const noexcept
    {
        return WindowBackend::headless;
    }

    std::shared_ptr<Window> HeadlessPlatformBackend::create_window(const WindowDesc &desc)
    {
        return std::make_shared<HeadlessWindow>(desc);
    }

    Task<std::shared_ptr<Window>> HeadlessPlatformBackend::create_window_async(const WindowDesc desc)
    {
        return create_task_from_result(create_window(desc));
    }

    std::shared_ptr<Window> HeadlessPlatformBackend::create_window_from_native(NativeWindowHandle handle)
    {
        return std::make_shared<HeadlessWindow>(handle);
    }

    Task<std::shared_ptr<Window>> HeadlessPlatformBackend::create_window_from_native_async(NativeWindowHandle handle)
    {
        return create_task_from_result(create_window_from_native(handle));
    }

    Optional<PlatformEvent> HeadlessPlatformBackend::poll_event()
    {
        return std::nullopt;
    }

    Optional<PlatformEvent> HeadlessPlatformBackend::wait_for_event()
    {
        return std::nullopt;
    }

    Optional<PlatformEvent> HeadlessPlatformBackend::wait_for_event(std::chrono::milliseconds timeout)
    {
        return std::nullopt;
    }

    void HeadlessPlatformBackend::push_event(PlatformEvent event)
    {
    }
} // namespace retro
