//
// Created by fcors on 12/28/2025.
//
module;

#define SDL_MAIN_HANDLED
#include <SDL3/SDL.h>
#include <SDL3/SDL_main.h>
#include <SDL3/SDL_vulkan.h>

export module sdl;

import std;
import vulkan_hpp;

namespace sdl
{
    export inline void set_main_ready()
    {
        SDL_SetMainReady();
    }

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
    
    export using Event = SDL_Event;
    
    export enum class EventType
    {
        FIRST = SDL_EVENT_FIRST,

        /* Application events */
        QUIT = SDL_EVENT_QUIT, /**< User-requested quit */

        /* These application events have special meaning on iOS and Android, see README-ios.md and README-android.md for details */
        TERMINATING = SDL_EVENT_TERMINATING,      /**< The application is being terminated by the OS. This event must be handled in a callback set with SDL_AddEventWatch().
                                         Called on iOS in applicationWillTerminate()
                                         Called on Android in onDestroy()
                                */
        LOW_MEMORY = SDL_EVENT_LOW_MEMORY,       /**< The application is low on memory, free memory if possible. This event must be handled in a callback set with SDL_AddEventWatch().
                                         Called on iOS in applicationDidReceiveMemoryWarning()
                                         Called on Android in onTrimMemory()
                                    */
        WILL_ENTER_BACKGROUND = SDL_EVENT_WILL_ENTER_BACKGROUND, /**< The application is about to enter the background. This event must be handled in a callback set with SDL_AddEventWatch().
                                         Called on iOS in applicationWillResignActive()
                                         Called on Android in onPause()
                                    */
        DID_ENTER_BACKGROUND = SDL_EVENT_DID_ENTER_BACKGROUND, /**< The application did enter the background and may not get CPU for some time. This event must be handled in a callback set with SDL_AddEventWatch().
                                         Called on iOS in applicationDidEnterBackground()
                                         Called on Android in onPause()
                                    */
        WILL_ENTER_FOREGROUND = SDL_EVENT_WILL_ENTER_FOREGROUND, /**< The application is about to enter the foreground. This event must be handled in a callback set with SDL_AddEventWatch().
                                         Called on iOS in applicationWillEnterForeground()
                                         Called on Android in onResume()
                                    */
        DID_ENTER_FOREGROUND = SDL_EVENT_DID_ENTER_FOREGROUND, /**< The application is now interactive. This event must be handled in a callback set with SDL_AddEventWatch().
                                         Called on iOS in applicationDidBecomeActive()
                                         Called on Android in onResume()
                                    */

        LOCALE_CHANGED = SDL_EVENT_LOCALE_CHANGED,  /**< The user's locale preferences have changed. */

        SYSTEM_THEME_CHANGED = SDL_EVENT_SYSTEM_THEME_CHANGED, /**< The system theme changed */

        /* Display events */
        /* 0x150 was SDL_DISPLAYEVENT, reserve the number for sdl2-compat */
        DISPLAY_ORIENTATION = SDL_EVENT_DISPLAY_ORIENTATION,   /**< Display orientation has changed to data1 */
        DISPLAY_ADDED = SDL_EVENT_DISPLAY_ADDED,                 /**< Display has been added to the system */
        DISPLAY_REMOVED = SDL_EVENT_DISPLAY_REMOVED,               /**< Display has been removed from the system */
        DISPLAY_MOVED = SDL_EVENT_DISPLAY_MOVED,                 /**< Display has changed position */
        DISPLAY_DESKTOP_MODE_CHANGED = SDL_EVENT_DISPLAY_DESKTOP_MODE_CHANGED,  /**< Display has changed desktop mode */
        DISPLAY_CURRENT_MODE_CHANGED = SDL_EVENT_DISPLAY_CURRENT_MODE_CHANGED,  /**< Display has changed current mode */
        DISPLAY_CONTENT_SCALE_CHANGED = SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED, /**< Display has changed content scale */
        DISPLAY_FIRST = SDL_EVENT_DISPLAY_FIRST,
        DISPLAY_LAST = SDL_EVENT_DISPLAY_LAST,

        /* Window events */
        /* 0x200 was SDL_WINDOWEVENT, reserve the number for sdl2-compat */
        /* 0x201 was SDL_SYSWMEVENT, reserve the number for sdl2-compat */
        WINDOW_SHOWN = SDL_EVENT_WINDOW_SHOWN,     /**< Window has been shown */
        WINDOW_HIDDEN = SDL_EVENT_WINDOW_HIDDEN,            /**< Window has been hidden */
        WINDOW_EXPOSED = SDL_EVENT_WINDOW_EXPOSED,           /**< Window has been exposed and should be redrawn, and can be redrawn directly from event watchers for this event */
        WINDOW_MOVED = SDL_EVENT_WINDOW_MOVED,             /**< Window has been moved to data1, data2 */
        WINDOW_RESIZED = SDL_EVENT_WINDOW_RESIZED,           /**< Window has been resized to data1xdata2 */
        WINDOW_PIXEL_SIZE_CHANGED = SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED,/**< The pixel size of the window has changed to data1xdata2 */
        WINDOW_METAL_VIEW_RESIZED = SDL_EVENT_WINDOW_METAL_VIEW_RESIZED,/**< The pixel size of a Metal view associated with the window has changed */
        WINDOW_MINIMIZED = SDL_EVENT_WINDOW_MINIMIZED,         /**< Window has been minimized */
        WINDOW_MAXIMIZED = SDL_EVENT_WINDOW_MAXIMIZED,         /**< Window has been maximized */
        WINDOW_RESTORED = SDL_EVENT_WINDOW_RESTORED,          /**< Window has been restored to normal size and position */
        WINDOW_MOUSE_ENTER = SDL_EVENT_WINDOW_MOUSE_ENTER,       /**< Window has gained mouse focus */
        WINDOW_MOUSE_LEAVE = SDL_EVENT_WINDOW_MOUSE_LEAVE,       /**< Window has lost mouse focus */
        WINDOW_FOCUS_GAINED = SDL_EVENT_WINDOW_FOCUS_GAINED,      /**< Window has gained keyboard focus */
        WINDOW_FOCUS_LOST = SDL_EVENT_WINDOW_FOCUS_LOST,        /**< Window has lost keyboard focus */
        WINDOW_CLOSE_REQUESTED = SDL_EVENT_WINDOW_CLOSE_REQUESTED,   /**< The window manager requests that the window be closed */
        WINDOW_HIT_TEST = SDL_EVENT_WINDOW_HIT_TEST,          /**< Window had a hit test that wasn't SDL_HITTEST_NORMAL */
        WINDOW_ICCPROF_CHANGED = SDL_EVENT_WINDOW_ICCPROF_CHANGED,   /**< The ICC profile of the window's display has changed */
        WINDOW_DISPLAY_CHANGED = SDL_EVENT_WINDOW_DISPLAY_CHANGED,   /**< Window has been moved to display data1 */
        WINDOW_DISPLAY_SCALE_CHANGED = SDL_EVENT_WINDOW_DISPLAY_SCALE_CHANGED, /**< Window display scale has been changed */
        WINDOW_SAFE_AREA_CHANGED = SDL_EVENT_WINDOW_SAFE_AREA_CHANGED, /**< The window safe area has been changed */
        WINDOW_OCCLUDED = SDL_EVENT_WINDOW_OCCLUDED,          /**< The window has been occluded */
        WINDOW_ENTER_FULLSCREEN = SDL_EVENT_WINDOW_ENTER_FULLSCREEN,  /**< The window has entered fullscreen mode */
        WINDOW_LEAVE_FULLSCREEN = SDL_EVENT_WINDOW_LEAVE_FULLSCREEN,  /**< The window has left fullscreen mode */
        WINDOW_DESTROYED = SDL_EVENT_WINDOW_DESTROYED,         /**< The window with the associated ID is being or has been destroyed. If this message is being handled
                                                 in an event watcher, the window handle is still valid and can still be used to retrieve any properties
                                                 associated with the window. Otherwise, the handle has already been destroyed and all resources
                                                 associated with it are invalid */
        WINDOW_HDR_STATE_CHANGED = SDL_EVENT_WINDOW_HDR_STATE_CHANGED, /**< Window HDR properties have changed */
        WINDOW_FIRST = SDL_EVENT_WINDOW_FIRST,
        WINDOW_LAST = SDL_EVENT_WINDOW_LAST,

        /* Keyboard events */
        KEY_DOWN = SDL_EVENT_KEY_DOWN, /**< Key pressed */
        KEY_UP = SDL_EVENT_KEY_UP,                  /**< Key released */
        TEXT_EDITING = SDL_EVENT_TEXT_EDITING,            /**< Keyboard text editing (composition) */
        TEXT_INPUT = SDL_EVENT_TEXT_INPUT,              /**< Keyboard text input */
        KEYMAP_CHANGED = SDL_EVENT_KEYMAP_CHANGED,          /**< Keymap changed due to a system event such as an
                                                input language or keyboard layout change. */
        KEYBOARD_ADDED = SDL_EVENT_KEYBOARD_ADDED,          /**< A new keyboard has been inserted into the system */
        KEYBOARD_REMOVED = SDL_EVENT_KEYBOARD_REMOVED,        /**< A keyboard has been removed */
        TEXT_EDITING_CANDIDATES = SDL_EVENT_TEXT_EDITING_CANDIDATES, /**< Keyboard text editing candidates */

        /* Mouse events */
        MOUSE_MOTION = SDL_EVENT_MOUSE_MOTION, /**< Mouse moved */
        MOUSE_BUTTON_DOWN = SDL_EVENT_MOUSE_BUTTON_DOWN,       /**< Mouse button pressed */
        MOUSE_BUTTON_UP = SDL_EVENT_MOUSE_BUTTON_UP,         /**< Mouse button released */
        MOUSE_WHEEL = SDL_EVENT_MOUSE_WHEEL,             /**< Mouse wheel motion */
        MOUSE_ADDED = SDL_EVENT_MOUSE_ADDED,             /**< A new mouse has been inserted into the system */
        MOUSE_REMOVED = SDL_EVENT_MOUSE_REMOVED,           /**< A mouse has been removed */

        /* Joystick events */
        JOYSTICK_AXIS_MOTION = SDL_EVENT_JOYSTICK_AXIS_MOTION, /**< Joystick axis motion */
        JOYSTICK_BALL_MOTION = SDL_EVENT_JOYSTICK_BALL_MOTION,          /**< Joystick trackball motion */
        JOYSTICK_HAT_MOTION = SDL_EVENT_JOYSTICK_HAT_MOTION,           /**< Joystick hat position change */
        JOYSTICK_BUTTON_DOWN = SDL_EVENT_JOYSTICK_BUTTON_DOWN,          /**< Joystick button pressed */
        JOYSTICK_BUTTON_UP = SDL_EVENT_JOYSTICK_BUTTON_UP,            /**< Joystick button released */
        JOYSTICK_ADDED = SDL_EVENT_JOYSTICK_ADDED,                /**< A new joystick has been inserted into the system */
        JOYSTICK_REMOVED = SDL_EVENT_JOYSTICK_REMOVED,              /**< An opened joystick has been removed */
        JOYSTICK_BATTERY_UPDATED = SDL_EVENT_JOYSTICK_BATTERY_UPDATED,      /**< Joystick battery level change */
        JOYSTICK_UPDATE_COMPLETE = SDL_EVENT_JOYSTICK_UPDATE_COMPLETE,      /**< Joystick update is complete */

        /* Gamepad events */
        GAMEPAD_AXIS_MOTION = SDL_EVENT_GAMEPAD_AXIS_MOTION, /**< Gamepad axis motion */
        GAMEPAD_BUTTON_DOWN = SDL_EVENT_GAMEPAD_BUTTON_DOWN,          /**< Gamepad button pressed */
        GAMEPAD_BUTTON_UP = SDL_EVENT_GAMEPAD_BUTTON_UP,            /**< Gamepad button released */
        GAMEPAD_ADDED = SDL_EVENT_GAMEPAD_ADDED,                /**< A new gamepad has been inserted into the system */
        GAMEPAD_REMOVED = SDL_EVENT_GAMEPAD_REMOVED,              /**< A gamepad has been removed */
        GAMEPAD_REMAPPED = SDL_EVENT_GAMEPAD_REMAPPED,             /**< The gamepad mapping was updated */
        GAMEPAD_TOUCHPAD_DOWN = SDL_EVENT_GAMEPAD_TOUCHPAD_DOWN,        /**< Gamepad touchpad was touched */
        GAMEPAD_TOUCHPAD_MOTION = SDL_EVENT_GAMEPAD_TOUCHPAD_MOTION,      /**< Gamepad touchpad finger was moved */
        GAMEPAD_TOUCHPAD_UP = SDL_EVENT_GAMEPAD_TOUCHPAD_UP,          /**< Gamepad touchpad finger was lifted */
        GAMEPAD_SENSOR_UPDATE = SDL_EVENT_GAMEPAD_SENSOR_UPDATE,        /**< Gamepad sensor was updated */
        GAMEPAD_UPDATE_COMPLETE = SDL_EVENT_GAMEPAD_UPDATE_COMPLETE,      /**< Gamepad update is complete */
        GAMEPAD_STEAM_HANDLE_UPDATED = SDL_EVENT_GAMEPAD_STEAM_HANDLE_UPDATED,  /**< Gamepad Steam handle has changed */

        /* Touch events */
        FINGER_DOWN = SDL_EVENT_FINGER_DOWN,
        FINGER_UP = SDL_EVENT_FINGER_UP,
        FINGER_MOTION = SDL_EVENT_FINGER_MOTION,
        FINGER_CANCELED = SDL_EVENT_FINGER_CANCELED,

        /* 0x800, 0x801, and 0x802 were the Gesture events from SDL2. Do not reuse these values! sdl2-compat needs them! */

        /* Clipboard events */
        CLIPBOARD_UPDATE = SDL_EVENT_CLIPBOARD_UPDATE, /**< The clipboard or primary selection changed */

        /* Drag and drop events */
        DROP_FILE = SDL_EVENT_DROP_FILE, /**< The system requests a file open */
        DROP_TEXT = SDL_EVENT_DROP_TEXT,                 /**< text/plain drag-and-drop event */
        DROP_BEGIN = SDL_EVENT_DROP_BEGIN,                /**< A new set of drops is beginning (NULL filename) */
        DROP_COMPLETE = SDL_EVENT_DROP_COMPLETE,             /**< Current set of drops is now complete (NULL filename) */
        DROP_POSITION = SDL_EVENT_DROP_POSITION,             /**< Position while moving over the window */

        /* Audio hotplug events */
        AUDIO_DEVICE_ADDED = SDL_EVENT_AUDIO_DEVICE_ADDED,  /**< A new audio device is available */
        AUDIO_DEVICE_REMOVED = SDL_EVENT_AUDIO_DEVICE_REMOVED,         /**< An audio device has been removed. */
        AUDIO_DEVICE_FORMAT_CHANGED = SDL_EVENT_AUDIO_DEVICE_FORMAT_CHANGED,  /**< An audio device's format has been changed by the system. */

        /* Sensor events */
        SENSOR_UPDATE = SDL_EVENT_SENSOR_UPDATE,     /**< A sensor was updated */

        /* Pressure-sensitive pen events */
        PEN_PROXIMITY_IN = SDL_EVENT_PEN_PROXIMITY_IN,  /**< Pressure-sensitive pen has become available */
        PEN_PROXIMITY_OUT = SDL_EVENT_PEN_PROXIMITY_OUT,          /**< Pressure-sensitive pen has become unavailable */
        PEN_DOWN = SDL_EVENT_PEN_DOWN,                   /**< Pressure-sensitive pen touched drawing surface */
        PEN_UP = SDL_EVENT_PEN_UP,                     /**< Pressure-sensitive pen stopped touching drawing surface */
        PEN_BUTTON_DOWN = SDL_EVENT_PEN_BUTTON_DOWN,            /**< Pressure-sensitive pen button pressed */
        PEN_BUTTON_UP = SDL_EVENT_PEN_BUTTON_UP,              /**< Pressure-sensitive pen button released */
        PEN_MOTION = SDL_EVENT_PEN_MOTION,                 /**< Pressure-sensitive pen is moving on the tablet */
        PEN_AXIS = SDL_EVENT_PEN_AXIS,                   /**< Pressure-sensitive pen angle/pressure/etc changed */

        /* Camera hotplug events */
        CAMERA_DEVICE_ADDED = SDL_EVENT_CAMERA_DEVICE_ADDED,  /**< A new camera device is available */
        CAMERA_DEVICE_REMOVED = SDL_EVENT_CAMERA_DEVICE_REMOVED,         /**< A camera device has been removed. */
        CAMERA_DEVICE_APPROVED = SDL_EVENT_CAMERA_DEVICE_APPROVED,        /**< A camera device has been approved for use by the user. */
        CAMERA_DEVICE_DENIED = SDL_EVENT_CAMERA_DEVICE_DENIED,          /**< A camera device has been denied for use by the user. */

        /* Render events */
        RENDER_TARGETS_RESET = SDL_EVENT_RENDER_TARGETS_RESET, /**< The render targets have been reset and their contents need to be updated */
        RENDER_DEVICE_RESET = SDL_EVENT_RENDER_DEVICE_RESET, /**< The device has been reset and all textures need to be recreated */
        RENDER_DEVICE_LOST = SDL_EVENT_RENDER_DEVICE_LOST, /**< The device has been lost and can't be recovered. */

        /* Reserved events for private platforms */
        PRIVATE0 = SDL_EVENT_PRIVATE0,
        PRIVATE1 = SDL_EVENT_PRIVATE1,
        PRIVATE2 = SDL_EVENT_PRIVATE2,
        PRIVATE3 = SDL_EVENT_PRIVATE3,

        /* Internal events */
        POLL_SENTINEL = SDL_EVENT_POLL_SENTINEL, /**< Signals the end of an event poll cycle */

        /** Events USER = SDL_EVENT_USER through LAST = SDL_EVENT_LAST are for your use,
         *  and should be allocated with SDL_RegisterEvents()
         */
        USER = SDL_EVENT_USER,

        /**
         *  This last event is only for bounding internal arrays
         */
        LAST = SDL_EVENT_LAST,

        /* This just makes sure the enum is the size of Uint32 */
        ENUM_PADDING = SDL_EVENT_ENUM_PADDING
    };

    export bool wait_event_timeout(Event& event, const int timeout)
    {
        return SDL_WaitEventTimeout(&event, timeout);
    }

    namespace vulkan
    {
        export inline std::span<const char* const> get_instance_extensions()
        {
            Uint32 count = 0;
            auto* names = check_error(SDL_Vulkan_GetInstanceExtensions(&count));
            return std::span(names, count);
        }
    }
}