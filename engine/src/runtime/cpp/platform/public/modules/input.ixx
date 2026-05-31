/**
 * @file input.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.input;

import std;
import retro.core.algorithm.hashing;

namespace retro
{
    export enum class LogicalKey : std::uint16_t
    {
        unknown,

        a,
        b,
        c,
        d,
        e,
        f,
        g,
        h,
        i,
        j,
        k,
        l,
        m,
        n,
        o,
        p,
        q,
        r,
        s,
        t,
        u,
        v,
        w,
        x,
        y,
        z,

        number0,
        number1,
        number2,
        number3,
        number4,
        number5,
        number6,
        number7,
        number8,
        number9,

        backtick,
        tilde,
        exclamation_mark,
        at,
        hash,
        dollar,
        percent,
        caret,
        ampersand,
        asterisk,
        open_paren,
        close_paren,
        minus,
        underscore,
        equals,
        plus,
        open_bracket,
        open_brace,
        close_bracket,
        close_brace,
        backslash,
        pipe,
        semicolon,
        colon,
        single_quote,
        double_quote,
        comma,
        less_than,
        period,
        greater_than,
        slash,
        question_mark,

        escape,
        enter,
        tab,
        backspace,
        space,
        caps_lock,

        left_shift,
        right_shift,
        left_control,
        right_control,
        left_alt,
        right_alt,

        print_screen,
        scroll_lock,
        pause,
        insert,
        home,
        del,
        end,
        page_up,
        page_down,

        arrow_up,
        arrow_down,
        arrow_left,
        arrow_right,

        num_lock,
        keypad_divide,
        keypad_multiply,
        keypad_minus,
        keypad_plus,
        keypad_enter,
        keypad_1,
        keypad_2,
        keypad_3,
        keypad_4,
        keypad_5,
        keypad_6,
        keypad_7,
        keypad_8,
        keypad_9,
        keypad_0,
        keypad_period,

        f1,
        f2,
        f3,
        f4,
        f5,
        f6,
        f7,
        f8,
        f9,
        f10,
        f11,
        f12
    };

    export constexpr inline std::size_t logical_key_max = static_cast<std::size_t>(LogicalKey::f12) + 1;

    export enum class PhysicalKey : std::uint16_t
    {
        unknown,

        a,
        b,
        c,
        d,
        e,
        f,
        g,
        h,
        i,
        j,
        k,
        l,
        m,
        n,
        o,
        p,
        q,
        r,
        s,
        t,
        u,
        v,
        w,
        x,
        y,
        z,

        number0,
        number1,
        number2,
        number3,
        number4,
        number5,
        number6,
        number7,
        number8,
        number9,

        backtick,
        minus,
        equals,
        open_bracket,
        close_bracket,
        backslash,
        semicolon,
        single_quote,
        comma,
        period,
        slash,

        escape,
        enter,
        tab,
        backspace,
        space,
        caps_lock,

        left_shift,
        right_shift,
        left_control,
        right_control,
        left_alt,
        right_alt,

        print_screen,
        scroll_lock,
        pause,
        insert,
        home,
        del,
        end,
        page_up,
        page_down,

        arrow_up,
        arrow_down,
        arrow_left,
        arrow_right,

        num_lock,
        keypad_divide,
        keypad_multiply,
        keypad_minus,
        keypad_plus,
        keypad_enter,
        keypad_1,
        keypad_2,
        keypad_3,
        keypad_4,
        keypad_5,
        keypad_6,
        keypad_7,
        keypad_8,
        keypad_9,
        keypad_0,
        keypad_period,

        f1,
        f2,
        f3,
        f4,
        f5,
        f6,
        f7,
        f8,
        f9,
        f10,
        f11,
        f12
    };

    export constexpr inline std::size_t physical_key_max = static_cast<std::size_t>(PhysicalKey::f12) + 1;

    export enum class MouseButton : std::uint8_t
    {
        left,
        middle,
        right,
        x1,
        x2,
        unknown,
    };

    export constexpr inline std::size_t mouse_button_max = static_cast<std::size_t>(MouseButton::x2) + 1;

    export enum class MouseWheelDirection : std::uint8_t
    {
        normal,
        flipped
    };

    export enum class GamepadChangeType : std::uint8_t
    {
        connected,
        disconnected,
    };

    export enum class GamepadButton : std::uint8_t
    {
        unknown,

        face_top,
        face_right,
        face_bottom,
        face_left,

        pad_up,
        pad_down,
        pad_left,
        pad_right,

        left_shoulder,
        right_shoulder,

        left_paddle1,
        left_paddle2,
        right_paddle1,
        right_paddle2,

        start,
        select,
        guide,

        left_thumbstick,
        right_thumbstick,

        touchpad,
        misc1,
        misc2,
        misc3,
        misc4,
        misc5,
        misc6,
    };

    export constexpr inline std::size_t gamepad_button_max = static_cast<std::size_t>(GamepadButton::misc6) + 1;

    export enum class GamepadAxis : std::uint8_t
    {
        unknown,

        left_x,
        left_y,
        right_x,
        right_y,

        left_trigger,
        right_trigger,
    };

    export constexpr inline std::size_t gamepad_axis_max = static_cast<std::size_t>(GamepadAxis::right_trigger) + 1;
} // namespace retro
