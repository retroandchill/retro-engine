//
// Created by fcors on 12/28/2025.
//
module;

#include <SDL3/SDL.h>
#include <SDL3/SDL_vulkan.h>

export module sdl;

import std;
import vulkan_hpp;

namespace sdl
{
    export class SdlException : public std::runtime_error
    {
        using std::runtime_error::runtime_error;
    };

    inline void check_error(const bool condition)
    {
        if (!condition)
        {
            throw SdlException{SDL_GetError()};
        }
    }

    template <typename T>
    T* check_error(T* ptr)
    {
        if (ptr == nullptr)
        {
            throw SdlException{SDL_GetError()};
        }

        return ptr;
    }

    export using InitFlags = SDL_InitFlags;

    export constexpr InitFlags INIT_VIDEO = SDL_INIT_AUDIO;
    export constexpr InitFlags INIT_JOYSTICK = SDL_INIT_JOYSTICK;
    export constexpr InitFlags INIT_HAPTIC = SDL_INIT_HAPTIC;
    export constexpr InitFlags INIT_GAMEPAD = SDL_INIT_GAMEPAD;
    export constexpr InitFlags INIT_EVENTS = SDL_INIT_EVENTS;
    export constexpr InitFlags INIT_SENSOR = SDL_INIT_SENSOR;
    export constexpr InitFlags INIT_CAMERA = SDL_INIT_CAMERA;

    export class InitGuard
    {
    public:
        explicit inline InitGuard(const InitFlags flags)
        {
            check_error(SDL_Init(flags));
        }

        InitGuard(const InitGuard &) = delete;
        InitGuard(InitGuard &&) = delete;

        inline ~InitGuard() noexcept
        {
            SDL_Quit();
        }

        InitGuard &operator=(const InitGuard &) = delete;
        InitGuard &operator=(InitGuard &&) = delete;
    };

    export struct Size
    {
        int width;
        int height;
    };

    template <typename T>
    concept HasCType = requires
    {
        typename T::CType;
    };

    template <typename T>
    concept PtrHandle = HasCType<T>
        && std::equality_comparable<T>
        && std::equality_comparable_with<T, std::add_pointer_t<typename T::CType>>
        && std::equality_comparable_with<T, std::nullptr_t>
        && std::convertible_to<T, std::add_pointer_t<typename T::CType>>
        && std::convertible_to<std::nullptr_t, T>;

    export template <PtrHandle T, auto DestroyFn>
        requires std::invocable<decltype(DestroyFn), std::add_pointer_t<typename T::CType>>
    class UniqueHandle
    {
    public:
        UniqueHandle() = default;

        explicit inline UniqueHandle(T handle) : handle_(handle)
        {

        }

        UniqueHandle(const UniqueHandle &) = delete;
        inline UniqueHandle(UniqueHandle &&other) noexcept : handle_{other.release()}
        {

        }

        ~UniqueHandle() noexcept
        {
            if (handle_ != nullptr)
                DestroyFn(handle_);
        }

        UniqueHandle &operator=(const UniqueHandle &) = delete;
        inline UniqueHandle &operator=(UniqueHandle &&other) noexcept
        {
            if (this != &other)
            {
                reset(other.release());
            }

            return *this;
        }

        UniqueHandle &operator=(T new_handle) noexcept
        {
            reset(new_handle);
            return *this;
        }

        [[nodiscard]] inline T release() noexcept
        {
            auto result = handle_;
            reset();
            return result;
        }

        inline void reset(T new_handle = nullptr) noexcept
        {
            if (handle_ != nullptr)
                DestroyFn(handle_);

            handle_ = new_handle;
        }

        inline void swap(UniqueHandle &other) noexcept
        {
            std::swap(handle_, other.handle_);
        }

        [[nodiscard]] inline T get() const noexcept
        {
            return handle_;
        }

        [[nodiscard]] static inline decltype(auto) get_deleter()
        {
            return DestroyFn;
        }

        [[nodiscard]] inline T *operator->() noexcept
        {
            return &handle_;
        }

        [[nodiscard]] inline const T *operator->() const noexcept
        {
            return &handle_;
        }

        [[nodiscard]] inline T operator*() const noexcept
        {
            return handle_;
        }

    private:
        T handle_{nullptr};
    };

    export class WindowView
    {
    public:
        using CType = SDL_Window;

        WindowView() = default;

        explicit inline WindowView(SDL_Window *window) : window_{window}
        {

        }

        explicit(false) inline WindowView(std::nullptr_t) noexcept
        {

        }

        [[nodiscard]] inline vk::UniqueSurfaceKHR create_vulkan_surface(const vk::Instance instance) const
        {
            VkSurfaceKHR surface;
            check_error(SDL_Vulkan_CreateSurface(window_, instance, nullptr, &surface));
            return vk::UniqueSurfaceKHR{surface, instance};
        }

        inline void set_title(const char *title) const noexcept
        {
            SDL_SetWindowTitle(window_, title);
        }

        [[nodiscard]] inline Size size() const noexcept
        {
            Size size{};
            SDL_GetWindowSize(window_, &size.width, &size.height);
            return size;
        }

        inline explicit(false) operator SDL_Window *() const noexcept
        {
            return window_;
        }

        [[nodiscard]] constexpr friend bool operator==(const WindowView &lhs, const WindowView &rhs) noexcept = default;

        [[nodiscard]] inline friend bool operator==(const WindowView &lhs, std::nullptr_t) noexcept
        {
            return lhs.window_ == nullptr;
        }

    private:
        SDL_Window *window_{nullptr};
    };

    export using Window = UniqueHandle<WindowView, &SDL_DestroyWindow>;

    export using WindowFlags = SDL_WindowFlags;

    export constexpr WindowFlags WINDOW_FULLSCREEN = SDL_WINDOW_FULLSCREEN;
    export constexpr WindowFlags WINDOW_OPENGL = SDL_WINDOW_OPENGL;
    export constexpr WindowFlags WINDOW_HIDDEN = SDL_WINDOW_HIDDEN;
    export constexpr WindowFlags WINDOW_BORDERLESS = SDL_WINDOW_BORDERLESS;
    export constexpr WindowFlags WINDOW_RESIZABLE = SDL_WINDOW_RESIZABLE;
    export constexpr WindowFlags WINDOW_MINIMIZED = SDL_WINDOW_MINIMIZED;
    export constexpr WindowFlags WINDOW_MAXIMIZED = SDL_WINDOW_MAXIMIZED;
    export constexpr WindowFlags WINDOW_MOUSE_GRABBED = SDL_WINDOW_MOUSE_GRABBED;
    export constexpr WindowFlags WINDOW_INPUT_FOCUS = SDL_WINDOW_INPUT_FOCUS;
    export constexpr WindowFlags WINDOW_MOUSE_FOCUS = SDL_WINDOW_MOUSE_FOCUS;
    export constexpr WindowFlags WINDOW_EXTERNAL = SDL_WINDOW_EXTERNAL;
    export constexpr WindowFlags WINDOW_MODAL = SDL_WINDOW_MODAL;
    export constexpr WindowFlags WINDOW_HIGH_PIXEL_DENSITY = SDL_WINDOW_HIGH_PIXEL_DENSITY;
    export constexpr WindowFlags WINDOW_MOUSE_CAPTURE = SDL_WINDOW_MOUSE_CAPTURE;
    export constexpr WindowFlags WINDOW_MOUSE_RELATIVE_MODE = SDL_WINDOW_MOUSE_RELATIVE_MODE;
    export constexpr WindowFlags WINDOW_ALWAYS_ON_TOP = SDL_WINDOW_ALWAYS_ON_TOP;
    export constexpr WindowFlags WINDOW_UTILITY = SDL_WINDOW_UTILITY;
    export constexpr WindowFlags WINDOW_METAL = SDL_WINDOW_METAL;
    export constexpr WindowFlags WINDOW_VULKAN = SDL_WINDOW_VULKAN;
    export constexpr WindowFlags WINDOW_TRANSPARENT = SDL_WINDOW_TRANSPARENT;
    export constexpr WindowFlags WINDOW_KEYBOARD_GRABBED = SDL_WINDOW_KEYBOARD_GRABBED;

    export inline Window create_window(const char* title, const int width, const int height, WindowFlags flags)
    {
        return Window{WindowView{check_error(SDL_CreateWindow(title, width, height, flags))}};
    }

}