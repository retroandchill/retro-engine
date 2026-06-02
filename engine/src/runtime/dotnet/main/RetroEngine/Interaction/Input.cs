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

    public static bool IsDown(DigitalInput input) => Manager.IsDown(input);

    public static bool IsAnyDown(params ReadOnlySpan<DigitalInput> inputs) => Manager.IsAnyDown(inputs);

    public static bool AreAllDown(params ReadOnlySpan<DigitalInput> inputs) => Manager.AreAllDown(inputs);

    public static bool AreNoneDown(params ReadOnlySpan<DigitalInput> inputs) => Manager.AreNoneDown(inputs);

    public static bool WasPressed(DigitalInput input) => Manager.WasPressed(input);

    public static bool WasAnyPressed(params ReadOnlySpan<DigitalInput> inputs) => Manager.WasAnyPressed(inputs);

    public static bool WereAllPressed(params ReadOnlySpan<DigitalInput> inputs) => Manager.WereAllPressed(inputs);

    public static bool WereNonePressed(params ReadOnlySpan<DigitalInput> inputs) => Manager.WereNonePressed(inputs);

    public static float GetAnalogueValue(AnalogueInput input) => Manager.GetAnalogueValue(input);

    public static float GetAnalogueValues(params ReadOnlySpan<AnalogueInput> inputs) =>
        Manager.GetAnalogueValues(inputs);
}
