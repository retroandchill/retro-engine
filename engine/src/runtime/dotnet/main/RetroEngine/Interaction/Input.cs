// @file Input.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Math;

namespace RetroEngine.Interaction;

public static class Input
{
    internal static InputManager Manager
    {
        get => field ?? throw new InvalidOperationException("Input manager not set.");
        set;
    }

    public static Vector2F MousePosition => Manager.MousePosition;

    public static Vector2F MouseDelta => Manager.MouseDelta;

    public static Vector2F MouseScrollDelta => Manager.MouseScrollDelta;

    public static bool IsDown(LogicalKey key) => Manager.IsDown(key);

    public static bool IsDown(PhysicalKey key) => Manager.IsDown(key);

    public static bool IsDown(MouseButton button) => Manager.IsDown(button);

    public static bool WasPressed(LogicalKey key) => Manager.WasPressed(key);

    public static bool WasPressed(PhysicalKey key) => Manager.WasPressed(key);

    public static bool WasPressed(MouseButton button) => Manager.WasPressed(button);
}
