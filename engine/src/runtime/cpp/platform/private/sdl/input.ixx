/**
 * @file input.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.sdl.input;

import std;
import sdl;
import retro.platform.input;

namespace retro
{
    namespace
    {
        constexpr std::array keycode_mappings = {
            std::make_pair(SDL::KEYCODE_UNKNOWN, LogicalKey::unknown),
            std::make_pair(SDL::KEYCODE_RETURN, LogicalKey::enter),
            std::make_pair(SDL::KEYCODE_ESCAPE, LogicalKey::escape),
            std::make_pair(SDL::KEYCODE_BACKSPACE, LogicalKey::backspace),
            std::make_pair(SDL::KEYCODE_TAB, LogicalKey::tab),
            std::make_pair(SDL::KEYCODE_SPACE, LogicalKey::space),
            std::make_pair(SDL::KEYCODE_EXCLAIM, LogicalKey::exclamation_mark),
            std::make_pair(SDL::KEYCODE_DBLAPOSTROPHE, LogicalKey::double_quote),
            std::make_pair(SDL::KEYCODE_HASH, LogicalKey::hash),
            std::make_pair(SDL::KEYCODE_DOLLAR, LogicalKey::dollar),
            std::make_pair(SDL::KEYCODE_PERCENT, LogicalKey::percent),
            std::make_pair(SDL::KEYCODE_AMPERSAND, LogicalKey::ampersand),
            std::make_pair(SDL::KEYCODE_APOSTROPHE, LogicalKey::single_quote),
            std::make_pair(SDL::KEYCODE_LEFTPAREN, LogicalKey::open_paren),
            std::make_pair(SDL::KEYCODE_RIGHTPAREN, LogicalKey::close_paren),
            std::make_pair(SDL::KEYCODE_ASTERISK, LogicalKey::asterisk),
            std::make_pair(SDL::KEYCODE_PLUS, LogicalKey::plus),
            std::make_pair(SDL::KEYCODE_COMMA, LogicalKey::comma),
            std::make_pair(SDL::KEYCODE_MINUS, LogicalKey::minus),
            std::make_pair(SDL::KEYCODE_PERIOD, LogicalKey::period),
            std::make_pair(SDL::KEYCODE_SLASH, LogicalKey::slash),
            std::make_pair(SDL::KEYCODE_0, LogicalKey::number0),
            std::make_pair(SDL::KEYCODE_1, LogicalKey::number1),
            std::make_pair(SDL::KEYCODE_2, LogicalKey::number2),
            std::make_pair(SDL::KEYCODE_3, LogicalKey::number3),
            std::make_pair(SDL::KEYCODE_4, LogicalKey::number4),
            std::make_pair(SDL::KEYCODE_5, LogicalKey::number5),
            std::make_pair(SDL::KEYCODE_6, LogicalKey::number6),
            std::make_pair(SDL::KEYCODE_7, LogicalKey::number7),
            std::make_pair(SDL::KEYCODE_8, LogicalKey::number8),
            std::make_pair(SDL::KEYCODE_9, LogicalKey::number9),
            std::make_pair(SDL::KEYCODE_COLON, LogicalKey::colon),
            std::make_pair(SDL::KEYCODE_SEMICOLON, LogicalKey::semicolon),
            std::make_pair(SDL::KEYCODE_LESS, LogicalKey::less_than),
            std::make_pair(SDL::KEYCODE_EQUALS, LogicalKey::equals),
            std::make_pair(SDL::KEYCODE_GREATER, LogicalKey::greater_than),
            std::make_pair(SDL::KEYCODE_QUESTION, LogicalKey::question_mark),
            std::make_pair(SDL::KEYCODE_AT, LogicalKey::at),
            std::make_pair(SDL::KEYCODE_LEFTBRACKET, LogicalKey::open_bracket),
            std::make_pair(SDL::KEYCODE_BACKSLASH, LogicalKey::backslash),
            std::make_pair(SDL::KEYCODE_RIGHTBRACKET, LogicalKey::close_bracket),
            std::make_pair(SDL::KEYCODE_CARET, LogicalKey::caret),
            std::make_pair(SDL::KEYCODE_UNDERSCORE, LogicalKey::underscore),
            std::make_pair(SDL::KEYCODE_GRAVE, LogicalKey::backtick),
            std::make_pair(SDL::KEYCODE_A, LogicalKey::a),
            std::make_pair(SDL::KEYCODE_B, LogicalKey::b),
            std::make_pair(SDL::KEYCODE_C, LogicalKey::c),
            std::make_pair(SDL::KEYCODE_D, LogicalKey::d),
            std::make_pair(SDL::KEYCODE_E, LogicalKey::e),
            std::make_pair(SDL::KEYCODE_F, LogicalKey::f),
            std::make_pair(SDL::KEYCODE_G, LogicalKey::g),
            std::make_pair(SDL::KEYCODE_H, LogicalKey::h),
            std::make_pair(SDL::KEYCODE_I, LogicalKey::i),
            std::make_pair(SDL::KEYCODE_J, LogicalKey::j),
            std::make_pair(SDL::KEYCODE_K, LogicalKey::k),
            std::make_pair(SDL::KEYCODE_L, LogicalKey::l),
            std::make_pair(SDL::KEYCODE_M, LogicalKey::m),
            std::make_pair(SDL::KEYCODE_N, LogicalKey::n),
            std::make_pair(SDL::KEYCODE_O, LogicalKey::o),
            std::make_pair(SDL::KEYCODE_P, LogicalKey::p),
            std::make_pair(SDL::KEYCODE_Q, LogicalKey::q),
            std::make_pair(SDL::KEYCODE_R, LogicalKey::r),
            std::make_pair(SDL::KEYCODE_S, LogicalKey::s),
            std::make_pair(SDL::KEYCODE_T, LogicalKey::t),
            std::make_pair(SDL::KEYCODE_U, LogicalKey::u),
            std::make_pair(SDL::KEYCODE_V, LogicalKey::v),
            std::make_pair(SDL::KEYCODE_W, LogicalKey::w),
            std::make_pair(SDL::KEYCODE_X, LogicalKey::x),
            std::make_pair(SDL::KEYCODE_Y, LogicalKey::y),
            std::make_pair(SDL::KEYCODE_Z, LogicalKey::z),
            std::make_pair(SDL::KEYCODE_LEFTBRACE, LogicalKey::open_brace),
            std::make_pair(SDL::KEYCODE_PIPE, LogicalKey::pipe),
            std::make_pair(SDL::KEYCODE_RIGHTBRACE, LogicalKey::close_brace),
            std::make_pair(SDL::KEYCODE_TILDE, LogicalKey::tilde),
            std::make_pair(SDL::KEYCODE_DELETE, LogicalKey::del),
            std::make_pair(SDL::KEYCODE_CAPSLOCK, LogicalKey::caps_lock),
            std::make_pair(SDL::KEYCODE_F1, LogicalKey::f1),
            std::make_pair(SDL::KEYCODE_F2, LogicalKey::f2),
            std::make_pair(SDL::KEYCODE_F3, LogicalKey::f3),
            std::make_pair(SDL::KEYCODE_F4, LogicalKey::f4),
            std::make_pair(SDL::KEYCODE_F5, LogicalKey::f5),
            std::make_pair(SDL::KEYCODE_F6, LogicalKey::f6),
            std::make_pair(SDL::KEYCODE_F7, LogicalKey::f7),
            std::make_pair(SDL::KEYCODE_F8, LogicalKey::f8),
            std::make_pair(SDL::KEYCODE_F9, LogicalKey::f9),
            std::make_pair(SDL::KEYCODE_F10, LogicalKey::f10),
            std::make_pair(SDL::KEYCODE_F11, LogicalKey::f11),
            std::make_pair(SDL::KEYCODE_F12, LogicalKey::f12),
            std::make_pair(SDL::KEYCODE_PRINTSCREEN, LogicalKey::print_screen),
            std::make_pair(SDL::KEYCODE_SCROLLLOCK, LogicalKey::scroll_lock),
            std::make_pair(SDL::KEYCODE_PAUSE, LogicalKey::pause),
            std::make_pair(SDL::KEYCODE_INSERT, LogicalKey::insert),
            std::make_pair(SDL::KEYCODE_HOME, LogicalKey::home),
            std::make_pair(SDL::KEYCODE_END, LogicalKey::end),
            std::make_pair(SDL::KEYCODE_PAGEUP, LogicalKey::page_up),
            std::make_pair(SDL::KEYCODE_PAGEDOWN, LogicalKey::page_down),
            std::make_pair(SDL::KEYCODE_RIGHT, LogicalKey::arrow_right),
            std::make_pair(SDL::KEYCODE_LEFT, LogicalKey::arrow_left),
            std::make_pair(SDL::KEYCODE_DOWN, LogicalKey::arrow_down),
            std::make_pair(SDL::KEYCODE_UP, LogicalKey::arrow_up),
            std::make_pair(SDL::KEYCODE_NUMLOCKCLEAR, LogicalKey::num_lock),
            std::make_pair(SDL::KEYCODE_KP_DIVIDE, LogicalKey::keypad_divide),
            std::make_pair(SDL::KEYCODE_KP_MULTIPLY, LogicalKey::keypad_multiply),
            std::make_pair(SDL::KEYCODE_KP_MINUS, LogicalKey::keypad_minus),
            std::make_pair(SDL::KEYCODE_KP_PLUS, LogicalKey::keypad_plus),
            std::make_pair(SDL::KEYCODE_KP_ENTER, LogicalKey::keypad_enter),
            std::make_pair(SDL::KEYCODE_KP_1, LogicalKey::keypad_1),
            std::make_pair(SDL::KEYCODE_KP_2, LogicalKey::keypad_2),
            std::make_pair(SDL::KEYCODE_KP_3, LogicalKey::keypad_3),
            std::make_pair(SDL::KEYCODE_KP_4, LogicalKey::keypad_4),
            std::make_pair(SDL::KEYCODE_KP_5, LogicalKey::keypad_5),
            std::make_pair(SDL::KEYCODE_KP_6, LogicalKey::keypad_6),
            std::make_pair(SDL::KEYCODE_KP_7, LogicalKey::keypad_7),
            std::make_pair(SDL::KEYCODE_KP_8, LogicalKey::keypad_8),
            std::make_pair(SDL::KEYCODE_KP_9, LogicalKey::keypad_9),
            std::make_pair(SDL::KEYCODE_KP_0, LogicalKey::keypad_0),
            std::make_pair(SDL::KEYCODE_KP_PERIOD, LogicalKey::keypad_period),
            std::make_pair(SDL::KEYCODE_LSHIFT, LogicalKey::left_shift),
            std::make_pair(SDL::KEYCODE_RSHIFT, LogicalKey::right_shift),
            std::make_pair(SDL::KEYCODE_LCTRL, LogicalKey::left_control),
            std::make_pair(SDL::KEYCODE_RCTRL, LogicalKey::right_control),
            std::make_pair(SDL::KEYCODE_LALT, LogicalKey::left_alt),
            std::make_pair(SDL::KEYCODE_RALT, LogicalKey::right_alt),
        };

        template <typename K, typename V, std::size_t N>
        consteval auto get_linear_enum_mapping(const std::array<std::pair<K, V>, N> &array)
        {
            std::array<K, N> result{};
            for (auto [key, value] : array)
            {
                result[std::to_underlying(value)] = key;
            }
            return result;
        }

        template <typename K, typename V, std::size_t N>
        consteval auto get_reverse_linear_enum_mapping(const std::array<std::pair<K, V>, N> &array, std::int32_t offset)
        {
            std::array<V, N> result{};
            for (auto [key, value] : array)
            {
                result[key - offset] = value;
            }
            return result;
        }

        constexpr inline auto logical_key_to_sdl_keycode_mapping = get_linear_enum_mapping(keycode_mappings);

        template <typename K, typename V, std::size_t N>
        consteval auto get_sorted_enum_mappings_mappings(const std::array<std::pair<K, V>, N> &array)
        {
            auto result = array;
            std::ranges::sort(result, [](const auto &lhs, const auto &rhs) { return lhs.first < rhs.first; });
            return result;
        }

        constexpr inline auto sorted_keycode_mappings = get_sorted_enum_mappings_mappings(keycode_mappings);

        constexpr std::array scancode_mappings = {
            std::make_pair(SDL::SCANCODE_UNKNOWN, PhysicalKey::unknown),
            std::make_pair(SDL::SCANCODE_A, PhysicalKey::a),
            std::make_pair(SDL::SCANCODE_B, PhysicalKey::b),
            std::make_pair(SDL::SCANCODE_C, PhysicalKey::c),
            std::make_pair(SDL::SCANCODE_D, PhysicalKey::d),
            std::make_pair(SDL::SCANCODE_E, PhysicalKey::e),
            std::make_pair(SDL::SCANCODE_F, PhysicalKey::f),
            std::make_pair(SDL::SCANCODE_G, PhysicalKey::g),
            std::make_pair(SDL::SCANCODE_H, PhysicalKey::h),
            std::make_pair(SDL::SCANCODE_I, PhysicalKey::i),
            std::make_pair(SDL::SCANCODE_J, PhysicalKey::j),
            std::make_pair(SDL::SCANCODE_K, PhysicalKey::k),
            std::make_pair(SDL::SCANCODE_L, PhysicalKey::l),
            std::make_pair(SDL::SCANCODE_M, PhysicalKey::m),
            std::make_pair(SDL::SCANCODE_N, PhysicalKey::n),
            std::make_pair(SDL::SCANCODE_O, PhysicalKey::o),
            std::make_pair(SDL::SCANCODE_P, PhysicalKey::p),
            std::make_pair(SDL::SCANCODE_Q, PhysicalKey::q),
            std::make_pair(SDL::SCANCODE_R, PhysicalKey::r),
            std::make_pair(SDL::SCANCODE_S, PhysicalKey::s),
            std::make_pair(SDL::SCANCODE_T, PhysicalKey::t),
            std::make_pair(SDL::SCANCODE_U, PhysicalKey::u),
            std::make_pair(SDL::SCANCODE_V, PhysicalKey::v),
            std::make_pair(SDL::SCANCODE_W, PhysicalKey::w),
            std::make_pair(SDL::SCANCODE_X, PhysicalKey::x),
            std::make_pair(SDL::SCANCODE_Y, PhysicalKey::y),
            std::make_pair(SDL::SCANCODE_Z, PhysicalKey::z),
            std::make_pair(SDL::SCANCODE_1, PhysicalKey::number1),
            std::make_pair(SDL::SCANCODE_2, PhysicalKey::number2),
            std::make_pair(SDL::SCANCODE_3, PhysicalKey::number3),
            std::make_pair(SDL::SCANCODE_4, PhysicalKey::number4),
            std::make_pair(SDL::SCANCODE_5, PhysicalKey::number5),
            std::make_pair(SDL::SCANCODE_6, PhysicalKey::number6),
            std::make_pair(SDL::SCANCODE_7, PhysicalKey::number7),
            std::make_pair(SDL::SCANCODE_8, PhysicalKey::number8),
            std::make_pair(SDL::SCANCODE_9, PhysicalKey::number9),
            std::make_pair(SDL::SCANCODE_0, PhysicalKey::number0),
            std::make_pair(SDL::SCANCODE_RETURN, PhysicalKey::enter),
            std::make_pair(SDL::SCANCODE_ESCAPE, PhysicalKey::escape),
            std::make_pair(SDL::SCANCODE_BACKSPACE, PhysicalKey::backspace),
            std::make_pair(SDL::SCANCODE_TAB, PhysicalKey::tab),
            std::make_pair(SDL::SCANCODE_SPACE, PhysicalKey::space),
            std::make_pair(SDL::SCANCODE_MINUS, PhysicalKey::minus),
            std::make_pair(SDL::SCANCODE_EQUALS, PhysicalKey::equals),
            std::make_pair(SDL::SCANCODE_LEFTBRACKET, PhysicalKey::open_bracket),
            std::make_pair(SDL::SCANCODE_RIGHTBRACKET, PhysicalKey::close_bracket),
            std::make_pair(SDL::SCANCODE_BACKSLASH, PhysicalKey::backslash),
            std::make_pair(SDL::SCANCODE_SEMICOLON, PhysicalKey::semicolon),
            std::make_pair(SDL::SCANCODE_APOSTROPHE, PhysicalKey::single_quote),
            std::make_pair(SDL::SCANCODE_GRAVE, PhysicalKey::backtick),
            std::make_pair(SDL::SCANCODE_COMMA, PhysicalKey::comma),
            std::make_pair(SDL::SCANCODE_PERIOD, PhysicalKey::period),
            std::make_pair(SDL::SCANCODE_SLASH, PhysicalKey::slash),
            std::make_pair(SDL::SCANCODE_CAPSLOCK, PhysicalKey::caps_lock),
            std::make_pair(SDL::SCANCODE_F1, PhysicalKey::f1),
            std::make_pair(SDL::SCANCODE_F2, PhysicalKey::f2),
            std::make_pair(SDL::SCANCODE_F3, PhysicalKey::f3),
            std::make_pair(SDL::SCANCODE_F4, PhysicalKey::f4),
            std::make_pair(SDL::SCANCODE_F5, PhysicalKey::f5),
            std::make_pair(SDL::SCANCODE_F6, PhysicalKey::f6),
            std::make_pair(SDL::SCANCODE_F7, PhysicalKey::f7),
            std::make_pair(SDL::SCANCODE_F8, PhysicalKey::f8),
            std::make_pair(SDL::SCANCODE_F9, PhysicalKey::f9),
            std::make_pair(SDL::SCANCODE_F10, PhysicalKey::f10),
            std::make_pair(SDL::SCANCODE_F11, PhysicalKey::f11),
            std::make_pair(SDL::SCANCODE_F12, PhysicalKey::f12),
            std::make_pair(SDL::SCANCODE_PRINTSCREEN, PhysicalKey::print_screen),
            std::make_pair(SDL::SCANCODE_SCROLLLOCK, PhysicalKey::scroll_lock),
            std::make_pair(SDL::SCANCODE_PAUSE, PhysicalKey::pause),
            std::make_pair(SDL::SCANCODE_INSERT, PhysicalKey::insert),
            std::make_pair(SDL::SCANCODE_HOME, PhysicalKey::home),
            std::make_pair(SDL::SCANCODE_END, PhysicalKey::end),
            std::make_pair(SDL::SCANCODE_DELETE, PhysicalKey::del),
            std::make_pair(SDL::SCANCODE_PAGEUP, PhysicalKey::page_up),
            std::make_pair(SDL::SCANCODE_PAGEDOWN, PhysicalKey::page_down),
            std::make_pair(SDL::SCANCODE_RIGHT, PhysicalKey::arrow_right),
            std::make_pair(SDL::SCANCODE_LEFT, PhysicalKey::arrow_left),
            std::make_pair(SDL::SCANCODE_DOWN, PhysicalKey::arrow_down),
            std::make_pair(SDL::SCANCODE_UP, PhysicalKey::arrow_up),
            std::make_pair(SDL::SCANCODE_NUMLOCKCLEAR, PhysicalKey::num_lock),
            std::make_pair(SDL::SCANCODE_KP_DIVIDE, PhysicalKey::keypad_divide),
            std::make_pair(SDL::SCANCODE_KP_MULTIPLY, PhysicalKey::keypad_multiply),
            std::make_pair(SDL::SCANCODE_KP_MINUS, PhysicalKey::keypad_minus),
            std::make_pair(SDL::SCANCODE_KP_PLUS, PhysicalKey::keypad_plus),
            std::make_pair(SDL::SCANCODE_KP_ENTER, PhysicalKey::keypad_enter),
            std::make_pair(SDL::SCANCODE_KP_1, PhysicalKey::keypad_1),
            std::make_pair(SDL::SCANCODE_KP_2, PhysicalKey::keypad_2),
            std::make_pair(SDL::SCANCODE_KP_3, PhysicalKey::keypad_3),
            std::make_pair(SDL::SCANCODE_KP_4, PhysicalKey::keypad_4),
            std::make_pair(SDL::SCANCODE_KP_5, PhysicalKey::keypad_5),
            std::make_pair(SDL::SCANCODE_KP_6, PhysicalKey::keypad_6),
            std::make_pair(SDL::SCANCODE_KP_7, PhysicalKey::keypad_7),
            std::make_pair(SDL::SCANCODE_KP_8, PhysicalKey::keypad_8),
            std::make_pair(SDL::SCANCODE_KP_9, PhysicalKey::keypad_9),
            std::make_pair(SDL::SCANCODE_KP_0, PhysicalKey::keypad_0),
            std::make_pair(SDL::SCANCODE_KP_PERIOD, PhysicalKey::keypad_period),
            std::make_pair(SDL::SCANCODE_LSHIFT, PhysicalKey::left_shift),
            std::make_pair(SDL::SCANCODE_RSHIFT, PhysicalKey::right_shift),
            std::make_pair(SDL::SCANCODE_LCTRL, PhysicalKey::left_control),
            std::make_pair(SDL::SCANCODE_RCTRL, PhysicalKey::right_control),
            std::make_pair(SDL::SCANCODE_LALT, PhysicalKey::left_alt),
            std::make_pair(SDL::SCANCODE_RALT, PhysicalKey::right_alt),
        };

        constexpr inline auto physical_key_to_scancode_mapping = get_linear_enum_mapping(scancode_mappings);

        constexpr inline auto sorted_scancode_mappings = get_sorted_enum_mappings_mappings(scancode_mappings);

        template <typename K, typename V, std::size_t N>
        constexpr V binary_search(const std::array<std::pair<K, V>, N> &array, K desired) noexcept
        {
            std::size_t low = 0;
            std::size_t high = N - 1;

            while (low <= high)
            {
                const std::size_t mid = low + (high - low) / 2;

                const auto &[key, value] = array[mid];
                if (key == desired)
                {
                    return value;
                }

                if (key < desired)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return V{};
        }

        constexpr std::array gamepad_button_mappings = {
            std::make_pair(SDL::GAMEPAD_BUTTON_INVALID, GamepadButton::unknown),
            std::make_pair(SDL::GAMEPAD_BUTTON_NORTH, GamepadButton::face_top),
            std::make_pair(SDL::GAMEPAD_BUTTON_EAST, GamepadButton::face_right),
            std::make_pair(SDL::GAMEPAD_BUTTON_SOUTH, GamepadButton::face_bottom),
            std::make_pair(SDL::GAMEPAD_BUTTON_WEST, GamepadButton::face_left),
            std::make_pair(SDL::GAMEPAD_BUTTON_DPAD_UP, GamepadButton::pad_up),
            std::make_pair(SDL::GAMEPAD_BUTTON_DPAD_DOWN, GamepadButton::pad_down),
            std::make_pair(SDL::GAMEPAD_BUTTON_DPAD_LEFT, GamepadButton::pad_left),
            std::make_pair(SDL::GAMEPAD_BUTTON_DPAD_RIGHT, GamepadButton::pad_right),
            std::make_pair(SDL::GAMEPAD_BUTTON_LEFT_SHOULDER, GamepadButton::left_shoulder),
            std::make_pair(SDL::GAMEPAD_BUTTON_RIGHT_SHOULDER, GamepadButton::right_shoulder),
            std::make_pair(SDL::GAMEPAD_BUTTON_LEFT_PADDLE1, GamepadButton::left_paddle1),
            std::make_pair(SDL::GAMEPAD_BUTTON_LEFT_PADDLE2, GamepadButton::left_paddle2),
            std::make_pair(SDL::GAMEPAD_BUTTON_RIGHT_PADDLE1, GamepadButton::right_paddle1),
            std::make_pair(SDL::GAMEPAD_BUTTON_RIGHT_PADDLE2, GamepadButton::right_paddle2),
            std::make_pair(SDL::GAMEPAD_BUTTON_START, GamepadButton::start),
            std::make_pair(SDL::GAMEPAD_BUTTON_BACK, GamepadButton::select),
            std::make_pair(SDL::GAMEPAD_BUTTON_GUIDE, GamepadButton::guide),
            std::make_pair(SDL::GAMEPAD_BUTTON_LEFT_STICK, GamepadButton::left_thumbstick),
            std::make_pair(SDL::GAMEPAD_BUTTON_RIGHT_STICK, GamepadButton::right_thumbstick),
            std::make_pair(SDL::GAMEPAD_BUTTON_TOUCHPAD, GamepadButton::touchpad),
            std::make_pair(SDL::GAMEPAD_BUTTON_MISC1, GamepadButton::misc1),
            std::make_pair(SDL::GAMEPAD_BUTTON_MISC2, GamepadButton::misc2),
            std::make_pair(SDL::GAMEPAD_BUTTON_MISC3, GamepadButton::misc3),
            std::make_pair(SDL::GAMEPAD_BUTTON_MISC4, GamepadButton::misc4),
            std::make_pair(SDL::GAMEPAD_BUTTON_MISC5, GamepadButton::misc5),
            std::make_pair(SDL::GAMEPAD_BUTTON_MISC6, GamepadButton::misc6),
        };

        constexpr inline auto engine_button_indices = get_linear_enum_mapping(gamepad_button_mappings);

        constexpr inline auto sdl_button_indices =
            get_reverse_linear_enum_mapping(gamepad_button_mappings, SDL::GAMEPAD_BUTTON_INVALID);
    } // namespace

    export constexpr MouseButton to_engine_mouse_button(const SDL::Uint8 sdl_button) noexcept
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

    export constexpr SDL::Uint8 to_sdl_mouse_button(const MouseButton button) noexcept
    {
        switch (button)
        {
            case MouseButton::left:
                return SDL::BUTTON_LEFT;
            case MouseButton::middle:
                return SDL::BUTTON_MIDDLE;
            case MouseButton::right:
                return SDL::BUTTON_RIGHT;
            case MouseButton::x1:
                return SDL::BUTTON_X1;
            case MouseButton::x2:
                return SDL::BUTTON_X2;
            case MouseButton::unknown:
                return 0;
        }

        return 0;
    }

    export constexpr SDL::Keycode to_sdl_keycode(const LogicalKey logical_key) noexcept
    {
        const auto underlying_logical_key = std::to_underlying(logical_key);
        if (underlying_logical_key >= logical_key_to_sdl_keycode_mapping.size())
            return SDL::KEYCODE_UNKNOWN;

        return logical_key_to_sdl_keycode_mapping[underlying_logical_key];
    }

    export constexpr LogicalKey to_logical_key(const SDL::Keycode keycode) noexcept
    {
        return binary_search(sorted_keycode_mappings, keycode);
    }

    export constexpr SDL::Scancode to_sdl_scancode(const PhysicalKey physical_key) noexcept
    {
        const auto underlying_physical_key = std::to_underlying(physical_key);
        if (underlying_physical_key >= physical_key_to_scancode_mapping.size())
            return SDL::SCANCODE_UNKNOWN;

        return physical_key_to_scancode_mapping[underlying_physical_key];
    }

    export constexpr PhysicalKey to_physical_key(const SDL::Scancode scancode) noexcept
    {
        return binary_search(sorted_scancode_mappings, scancode);
    }

    export constexpr SDL::GamepadButton to_sdl_gamepad_button(const GamepadButton button) noexcept
    {
        const auto underlying_button = std::to_underlying(button);
        if (underlying_button >= engine_button_indices.size())
            return SDL::GAMEPAD_BUTTON_INVALID;

        return engine_button_indices[underlying_button];
    }

    export constexpr GamepadButton to_engine_gamepad_button(const SDL::GamepadButton button) noexcept
    {
        const auto underlying_button = button - SDL::GAMEPAD_BUTTON_INVALID;
        if (underlying_button >= sdl_button_indices.size())
            return GamepadButton::unknown;

        return sdl_button_indices[underlying_button];
    }

    export constexpr SDL::GamepadAxis to_sdl_gamepad_axis(const GamepadAxis axis) noexcept
    {
        switch (axis)
        {
            case GamepadAxis::unknown:
                return SDL::GAMEPAD_AXIS_INVALID;
            case GamepadAxis::left_x:
                return SDL::GAMEPAD_AXIS_LEFTX;
            case GamepadAxis::left_y:
                return SDL::GAMEPAD_AXIS_LEFTY;
            case GamepadAxis::right_x:
                return SDL::GAMEPAD_AXIS_RIGHTX;
            case GamepadAxis::right_y:
                return SDL::GAMEPAD_AXIS_RIGHTY;
            case GamepadAxis::left_trigger:
                return SDL::GAMEPAD_AXIS_LEFT_TRIGGER;
            case GamepadAxis::right_trigger:
                return SDL::GAMEPAD_AXIS_RIGHT_TRIGGER;
        }

        return SDL::GAMEPAD_AXIS_INVALID;
    }

    export constexpr GamepadAxis to_engine_gamepad_axis(const SDL::GamepadAxis axis) noexcept
    {
        switch (axis)
        {
            case SDL::GAMEPAD_AXIS_LEFTX:
                return GamepadAxis::left_x;
            case SDL::GAMEPAD_AXIS_LEFTY:
                return GamepadAxis::left_y;
            case SDL::GAMEPAD_AXIS_RIGHTX:
                return GamepadAxis::right_x;
            case SDL::GAMEPAD_AXIS_RIGHTY:
                return GamepadAxis::right_y;
            case SDL::GAMEPAD_AXIS_LEFT_TRIGGER:
                return GamepadAxis::left_trigger;
            case SDL::GAMEPAD_AXIS_RIGHT_TRIGGER:
                return GamepadAxis::right_trigger;
            case SDL::GAMEPAD_AXIS_INVALID:
            case SDL::GAMEPAD_AXIS_COUNT:
            default:
                return GamepadAxis::unknown;
        }
    }

    export constexpr float normalize_gamepad_axis(const SDL::Sint16 value)
    {
        return value < 0 ? -static_cast<float>(value) / std::numeric_limits<SDL::Sint16>::min()
                         : static_cast<float>(value) / std::numeric_limits<SDL::Sint16>::max();
    }

    export constexpr SDL::Sint16 denormalize_gamepad_axis(const float value)
    {
        return static_cast<SDL::Sint16>(value < 0 ? std::round(-value * std::numeric_limits<SDL::Sint16>::max())
                                                  : std::round(value * std::numeric_limits<SDL::Sint16>::max()));
    }
} // namespace retro
