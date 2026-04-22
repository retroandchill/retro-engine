/**
 * @file headless.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.backend:headless;

import std;
import retro.core.async.future;
import retro.core.containers.optional;
import retro.core.functional.overload;
import retro.core.math.vector;
import retro.core.strings.cstring_view;
import retro.platform.backend;
import retro.platform.exceptions;
import retro.platform.window;
import retro.platform.event;

namespace retro
{
    namespace
    {
        std::atomic<std::uint64_t> next_window_id{1};
    }

    class HeadlessWindow final : public Window
    {
      public:
        explicit HeadlessWindow(const WindowDesc &desc)
            : size_{static_cast<std::uint32_t>(desc.width), static_cast<std::uint32_t>(desc.height)}
        {
        }

        [[nodiscard]] std::uint64_t id() const noexcept override
        {
            return id_;
        }

        [[nodiscard]] NativeWindowHandle native_handle() const noexcept override
        {
            return {.backend = WindowBackend::headless, .handle = nullptr};
        }

        void set_title(CStringView title) override
        {
            // Does nothing
        }

        [[nodiscard]] Vector2u size() const override
        {
            return size_;
        }

      private:
        std::uint64_t id_{next_window_id.fetch_add(1, std::memory_order_relaxed)};
        Vector2u size_;
    };

    class HeadlessPlatformBackend final : public PlatformBackend
    {
      public:
        [[nodiscard]] WindowBackend window_backend() const noexcept override
        {
            return WindowBackend::headless;
        }

        PlatformResult<std::shared_ptr<Window>> create_window(const WindowDesc &desc) override
        {
            return std::make_shared<HeadlessWindow>(desc);
        }

        Task<PlatformResult<std::shared_ptr<Window>>> create_window_async(const WindowDesc desc) override
        {
            return create_task_from_result(create_window(desc));
        }

        Optional<Event> poll_event() override
        {
            return std::nullopt;
        }

        Optional<Event> wait_for_event() override
        {
            return std::nullopt;
        }

        Optional<Event> wait_for_event(std::chrono::milliseconds timeout) override
        {
            return std::nullopt;
        }

        PlatformResult<void> push_event(Event event) override
        {
            return {};
        }
    };
} // namespace retro
