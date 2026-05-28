/**
 * @file sdl.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.backend:sdl;

import sdl;
import std;
import retro.core.async.future;
import retro.core.containers.optional;
import retro.core.functional.overload;
import retro.core.math.vector;
import retro.core.strings.cstring_view;
import retro.platform.backend;
import retro.platform.window;
import retro.platform.event;
import retro.core.util.noncopyable;
import retro.core.util.deferred;
import retro.core.util.exceptions;

namespace retro
{
    namespace
    {
        constexpr SDL::Uint32 to_sdl_flags(PlatformInitFlags flags) noexcept
        {
            SDL::Uint32 out = 0;

            const auto f = static_cast<std::uint32_t>(flags);

            if (f & static_cast<std::uint32_t>(PlatformInitFlags::audio))
                out |= SDL::INIT_AUDIO;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::video))
                out |= SDL::INIT_VIDEO;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::joystick))
                out |= SDL::INIT_JOYSTICK;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::haptic))
                out |= SDL::INIT_HAPTIC;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::gamepad))
                out |= SDL::INIT_GAMEPAD;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::events))
                out |= SDL::INIT_EVENTS;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::sensor))
                out |= SDL::INIT_SENSOR;
            if (f & static_cast<std::uint32_t>(PlatformInitFlags::camera))
                out |= SDL::INIT_CAMERA;

            return out;
        }

        constexpr SDL::WindowFlags to_sdl_window_flags(WindowFlags flags) noexcept
        {
            SDL::WindowFlags out = 0;

            const auto f = static_cast<std::uint64_t>(flags);

            if (f & static_cast<std::uint64_t>(WindowFlags::resizable))
                out |= SDL::WINDOW_RESIZABLE;
            if (f & static_cast<std::uint64_t>(WindowFlags::borderless))
                out |= SDL::WINDOW_BORDERLESS;
            if (f & static_cast<std::uint64_t>(WindowFlags::hidden))
                out |= SDL::WINDOW_HIDDEN;
            if (f & static_cast<std::uint64_t>(WindowFlags::vulkan))
                out |= SDL::WINDOW_VULKAN;
            if (f & static_cast<std::uint64_t>(WindowFlags::high_dpi))
                out |= SDL::WINDOW_HIGH_PIXEL_DENSITY;
            if (f & static_cast<std::uint64_t>(WindowFlags::always_on_top))
                out |= SDL::WINDOW_ALWAYS_ON_TOP;

            return out;
        }

        constexpr MouseButton to_mouse_button(const SDL::Uint8 sdl_button) noexcept
        {
            switch (sdl_button)
            {
                case SDL::BUTTON_LEFT:
                    return MouseButton::left;
                case SDL::BUTTON_MIDDLE:
                    return MouseButton::middle;
                case SDL::BUTTON_RIGHT:
                    return MouseButton::right;
                case SDL::BUTTON_X1:
                    return MouseButton::x1;
                case SDL::BUTTON_X2:
                    return MouseButton::x2;
                default:
                    return MouseButton::unknown;
            }
        }
    } // namespace

    class Sdl3Window final : public Window
    {
      public:
        explicit Sdl3Window(const WindowDesc &desc)
            : Sdl3Window{
                  SDL::CreateWindow(desc.title.data(), {desc.width, desc.height}, to_sdl_window_flags(desc.flags))}
        {
        }

        explicit Sdl3Window(NativeWindowHandle handle) : Sdl3Window{create_window_from_native(handle)}
        {
        }

      private:
        explicit Sdl3Window(SDL::Window window) : window_{std::move(window)}
        {
        }

      public:
        Sdl3Window(const Sdl3Window &) = delete;
        Sdl3Window(Sdl3Window &&) noexcept = delete;

        ~Sdl3Window() noexcept override
        {
            SDL::RunOnMainThread([w = window_.release()] { SDL::DestroyWindow(w); }, false);
        }

        Sdl3Window &operator=(const Sdl3Window &) = delete;
        Sdl3Window &operator=(Sdl3Window &&) noexcept = delete;

      public:
        [[nodiscard]] std::uint64_t id() const noexcept override
        {
            return window_.GetID();
        }

        [[nodiscard]] PlatformWindowHandle platform_handle() const noexcept override
        {
            return {.backend = WindowBackend::sdl3, .handle = window_.get()};
        }

        void set_title(const CStringView title) override
        {
            window_.SetTitle(title.data());
        }

        [[nodiscard]] Vector2u size() const override
        {
            int w = 0;
            int h = 0;
            window_.GetSize(&w, &h);
            return {static_cast<std::uint32_t>(w), static_cast<std::uint32_t>(h)};
        }

      private:
        static SDL::Window create_window_from_native(NativeWindowHandle handle)
        {
            auto properties = SDL::CreateProperties();
            switch (handle.type)
            {
                case NativeWindowType::win32_hwnd:
                    SDL::SetPointerProperty(properties, SDL::prop::Window::Create::WIN32_HWND_POINTER, handle.handle);
                    break;

                case NativeWindowType::x11_window:
                    SDL::SetNumberProperty(properties,
                                           SDL::prop::Window::Create::WIN32_HWND_POINTER,
                                           std::bit_cast<std::intptr_t>(handle.handle));
                    break;
                case NativeWindowType::wayland_surface:
                    SDL::SetPointerProperty(properties,
                                            SDL::prop::Window::Create::WAYLAND_WL_SURFACE_POINTER,
                                            handle.handle);
                    break;

                case NativeWindowType::cocoa_window:
                    SDL::SetPointerProperty(properties, SDL::prop::Window::Create::COCOA_WINDOW_POINTER, handle.handle);
                    break;
                case NativeWindowType::cocoa_view:
                    SDL::SetPointerProperty(properties, SDL::prop::Window::Create::COCOA_VIEW_POINTER, handle.handle);
                    break;

                case NativeWindowType::unknown:
                default:
                    throw PlatformException{"Unknown native window type"};
                    break;
            }

            return SDL::CreateWindowWithProperties(properties);
        }

        SDL::Window window_;
    };

    class Sdl3PlatformBackend final : public PlatformBackend
    {
      public:
        explicit Sdl3PlatformBackend(const PlatformInitFlags flags)
        {
            const SDL::Uint32 sdl_flags = to_sdl_flags(flags);
            SDL::Init(sdl_flags);

            if (has_any_flags(flags, PlatformInitFlags::video | PlatformInitFlags::events))
            {
                events_processing_enabled_ = true;
                const std::uint32_t first_event_flag = SDL::RegisterEvents(1);
                custom_event_types_.callback_event = first_event_flag;
            }
        }

        Sdl3PlatformBackend(const Sdl3PlatformBackend &) = delete;
        Sdl3PlatformBackend(Sdl3PlatformBackend &&) noexcept = delete;

        ~Sdl3PlatformBackend() noexcept override
        {
            SDL::Quit();
        }

        Sdl3PlatformBackend &operator=(const Sdl3PlatformBackend &) = delete;
        Sdl3PlatformBackend &operator=(Sdl3PlatformBackend &&) noexcept = delete;

        [[nodiscard]] WindowBackend window_backend() const noexcept override
        {
            return WindowBackend::sdl3;
        }

        std::shared_ptr<Window> create_window(const WindowDesc &desc) override
        {
            return create_window_internal(desc);
        }

        Task<std::shared_ptr<Window>> create_window_async(WindowDesc desc) override
        {
            return create_window_internal_async(std::move(desc));
        }

        std::shared_ptr<Window> create_window_from_native(NativeWindowHandle handle) override
        {
            return create_window_internal(handle);
        }

        Task<std::shared_ptr<Window>> create_window_from_native_async(NativeWindowHandle handle) override
        {
            return create_window_internal_async(handle);
        }

        Optional<PlatformEvent> poll_event() override
        {
            SDL::Event e{};
            if (!SDL::PollEvent(&e))
            {
                return std::nullopt;
            }

            return to_event(e);
        }

        Optional<PlatformEvent> wait_for_event() override
        {
            return to_event(SDL::WaitEvent());
        }

        Optional<PlatformEvent> wait_for_event(const std::chrono::milliseconds timeout) override
        {
            SDL::Event e{};
            if (!SDL::WaitEventTimeout(&e, timeout))
            {
                return std::nullopt;
            }

            return to_event(e);
        }

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

            Promise<std::shared_ptr<Window>> promise;
            push_event(CallbackEvent{.callback = [&promise, &args...]
                                     {
                                         promise.emplace(std::make_shared<Sdl3Window>(std::forward<Args>(args)...));
                                     }});

            return promise.get_future().get();
        }

        template <typename... Args>
            requires std::constructible_from<Sdl3Window, Args...>
        Task<std::shared_ptr<Window>> create_window_internal_async(Args &&...args)
        {
            if (SDL::IsMainThread())
            {
                co_return std::make_shared<Sdl3Window>(std::forward<Args>(args)...);
            }

            auto promise = std::make_shared<Promise<std::shared_ptr<Window>>>();
            push_event(CallbackEvent{.callback = [promise, ... args = std::forward<Args>(args)] mutable
                                     {
                                         promise->emplace(std::make_shared<Sdl3Window>(std::forward<Args>(args)...));
                                     }});

            co_return co_await promise->get_future();
        }

        SDL::Event from_event(PlatformEvent &&event)
        {
            return std::visit(
                Overload{[](const QuitEvent &)
                         {
                             return SDL::Event{
                                 .quit =
                                     {
                                         .type = SDL::EVENT_QUIT,
                                     },
                             };
                         },
                         [](const WindowCloseRequestedEvent &close_event)
                         {
                             return SDL::Event{
                                 .window =
                                     {
                                         .type = SDL::EVENT_WINDOW_CLOSE_REQUESTED,
                                         .windowID = close_event.window_id,
                                     },
                             };
                         },
                         [](const WindowResizedEvent &resize_event)
                         {
                             return SDL::Event{
                                 .window =
                                     {
                                         .type = SDL::EVENT_WINDOW_RESIZED,
                                         .windowID = resize_event.window_id,
                                         .data1 = resize_event.width,
                                         .data2 = resize_event.height,
                                     },
                             };
                         },
                         [](const MouseMovedEvent &move_event)
                         {
                             return SDL::Event{.motion = {
                                                   .type = SDL::EVENT_MOUSE_MOTION,
                                                   .windowID = move_event.window_id,
                                                   .x = move_event.x,
                                                   .y = move_event.y,
                                                   .xrel = move_event.dx,
                                                   .yrel = move_event.dy,
                                               }};
                         },
                         [](const MouseButtonEvent &button_event)
                         {
                             return SDL::Event{.button = {
                                                   .type = button_event.down ? SDL::EVENT_MOUSE_BUTTON_DOWN
                                                                             : SDL::EVENT_MOUSE_BUTTON_UP,
                                                   .windowID = button_event.window_id,
                                                   .button = static_cast<SDL::Uint8>(button_event.button),
                                                   .x = button_event.x,
                                                   .y = button_event.y,
                                               }};
                         },
                         [](const KeyEvent &key_event)
                         {
                             return SDL::Event{.key = {
                                                   .type = key_event.down ? SDL::EVENT_KEY_DOWN : SDL::EVENT_KEY_UP,
                                                   .windowID = key_event.window_id,
                                                   .scancode = static_cast<SDL::ScancodeRaw>(key_event.scancode),
                                                   .key = static_cast<SDL::Keycode>(key_event.keycode),
                                                   .repeat = key_event.repeat,
                                               }};
                         },
                         [this](CallbackEvent &&callback_event)
                         {
                             deferred_events_.push_back(std::move(callback_event.callback));
                             return SDL::Event{.user = {
                                                   .type = custom_event_types_.callback_event,
                                               }};
                         }},
                std::move(event));
        }

        Optional<PlatformEvent> to_event(const SDL::Event &e) noexcept
        {
            switch (e.type)
            {
                case SDL::EVENT_QUIT:
                    return PlatformEvent{QuitEvent{}};

                case SDL::EVENT_WINDOW_CLOSE_REQUESTED:
                    return PlatformEvent{WindowCloseRequestedEvent{
                        .window_id = e.window.windowID,
                    }};

                case SDL::EVENT_WINDOW_RESIZED:
                    return PlatformEvent{WindowResizedEvent{
                        .window_id = e.window.windowID,
                        .width = e.window.data1,
                        .height = e.window.data2,
                    }};

                case SDL::EVENT_MOUSE_MOTION:
                    return PlatformEvent{MouseMovedEvent{
                        .window_id = e.motion.windowID,
                        .x = e.motion.x,
                        .y = e.motion.y,
                        .dx = e.motion.xrel,
                        .dy = e.motion.yrel,
                    }};

                case SDL::EVENT_MOUSE_BUTTON_DOWN:
                case SDL::EVENT_MOUSE_BUTTON_UP:
                    return PlatformEvent{MouseButtonEvent{
                        .window_id = e.button.windowID,
                        .button = to_mouse_button(e.button.button),
                        .down = (e.type == SDL::EVENT_MOUSE_BUTTON_DOWN),
                        .x = e.button.x,
                        .y = e.button.y,
                    }};

                case SDL::EVENT_KEY_DOWN:
                case SDL::EVENT_KEY_UP:
                    return PlatformEvent{KeyEvent{
                        .window_id = e.key.windowID,
                        .keycode = static_cast<std::int32_t>(e.key.key),
                        .scancode = static_cast<std::int32_t>(e.key.scancode),
                        .down = (e.type == SDL::EVENT_KEY_DOWN),
                        .repeat = e.key.repeat,
                    }};

                default:
                    if (!events_processing_enabled_)
                        return std::nullopt;
                    if (e.type == custom_event_types_.callback_event)
                    {
                        auto callback = std::move(deferred_events_.front());
                        deferred_events_.pop_front();
                        return CallbackEvent{std::move(callback)};
                    }
                    return std::nullopt;
            }
        }

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
