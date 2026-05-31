// @file GamepadButton.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Interaction;

public enum GamepadButton : byte
{
    Unknown,

    FaceTop,
    FaceRight,
    FaceBottom,
    FaceLeft,

    PadUp,
    PadDown,
    PadLeft,
    PadRight,

    LeftShoulder,
    RightShoulder,

    LeftPaddle1,
    LeftPaddle2,
    RightPaddle1,
    RightPaddle2,

    Start,
    Select,
    Guide,

    LeftThumbstick,
    RightThumbstick,

    Touchpad,
    Misc1,
    Misc2,
    Misc3,
    Misc4,
    Misc5,
    Misc6,
}
