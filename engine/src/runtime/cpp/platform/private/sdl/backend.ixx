/**
 * @file backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.sdl.backend;

import std;
import sdl;
import retro.platform.backend;
import retro.platform.event;
import retro.platform.window;
import retro.platform.sdl.window;
import retro.core.containers.optional;
import retro.core.async.task;

namespace retro
{
    export class Sdl3PlatformBackend final : public PlatformBackend
    {
      public:
        explicit Sdl3PlatformBackend(PlatformInitFlags flags);

        Sdl3PlatformBackend(const Sdl3PlatformBackend &) = delete;
        Sdl3PlatformBackend(Sdl3PlatformBackend &&) noexcept = delete;

        ~Sdl3PlatformBackend() noexcept override;

        Sdl3PlatformBackend &operator=(const Sdl3PlatformBackend &) = delete;
        Sdl3PlatformBackend &operator=(Sdl3PlatformBackend &&) noexcept = delete;

        [[nodiscard]] WindowBackend window_backend() const noexcept override;

        std::shared_ptr<Window> create_window(const WindowDesc &desc) override;

        Task<std::shared_ptr<Window>> create_window_async(WindowDesc desc) override;

        std::shared_ptr<Window> create_window_from_native(NativeWindowHandle handle) override;

        Task<std::shared_ptr<Window>> create_window_from_native_async(NativeWindowHandle handle) override;

        Optional<PlatformEvent> poll_event() override;

        Optional<PlatformEvent> wait_for_event() override;

        Optional<PlatformEvent> wait_for_event(const std::chrono::milliseconds timeout) override;

        void push_event(PlatformEvent event) override
        {
            const auto e = from_event(std::move(event));
            SDL::PushEvent(e);
        }

      private:
        template <typename... Args>
            requires std::constructible_from<Sdl3Window, Args...>
        std::shared_ptr<Window> create_window_internal(Args &&...args)
        {
            if (SDL::IsMainThread())
            {
                return std::make_shared<Sdl3Window>(std::forward<Args>(args)...);
            }

            TaskCompletionSource<std::shared_ptr<Window>> promise;
            push_event(CallbackEvent{.callback = [&promise, &args...]
                                     {
                                         promise.emplace_result(
                                             std::make_shared<Sdl3Window>(std::forward<Args>(args)...));
                                     }});

            return promise.get_task().get();
        }

        template <typename... Args>
            requires std::constructible_from<Sdl3Window, Args...>
        Task<std::shared_ptr<Window>> create_window_internal_async(Args &&...args)
        {
            if (SDL::IsMainThread())
            {
                co_return std::make_shared<Sdl3Window>(std::forward<Args>(args)...);
            }

            TaskCompletionSource<std::shared_ptr<Window>> promise;
            push_event(CallbackEvent{.callback = [&promise, ... args = std::forward<Args>(args)] mutable
                                     {
                                         promise.emplace_result(
                                             std::make_shared<Sdl3Window>(std::forward<Args>(args)...));
                                     }});

            co_return co_await promise.get_task();
        }

        SDL::Event from_event(PlatformEvent &&event);

        Optional<PlatformEvent> to_event(const SDL::Event &e) noexcept;

      private:
        struct CustomEventTypes
        {
            std::uint32_t callback_event;
        };

        CustomEventTypes custom_event_types_{};
        bool events_processing_enabled_ = false;
        std::mutex event_queue_mutex_;
        std::deque<std::function<void()>> deferred_events_;
    };

} // namespace retro
