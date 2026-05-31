/**
 * @file backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.headless.backend;

import std;
import retro.core.containers.optional;
import retro.core.async.task;
import retro.platform.backend;
import retro.platform.event;
import retro.platform.window;

namespace retro
{
    export class HeadlessPlatformBackend final : public PlatformBackend
    {
      public:
        [[nodiscard]] WindowBackend window_backend() const noexcept override;

        std::shared_ptr<Window> create_window(const WindowDesc &desc) override;

        Task<std::shared_ptr<Window>> create_window_async(WindowDesc desc) override;

        std::shared_ptr<Window> create_window_from_native(NativeWindowHandle handle) override;

        Task<std::shared_ptr<Window>> create_window_from_native_async(NativeWindowHandle handle) override;

        Optional<PlatformEvent> poll_event() override;

        Optional<PlatformEvent> wait_for_event() override;

        Optional<PlatformEvent> wait_for_event(std::chrono::milliseconds timeout) override;

        void push_event(PlatformEvent event) override;
    };
} // namespace retro
