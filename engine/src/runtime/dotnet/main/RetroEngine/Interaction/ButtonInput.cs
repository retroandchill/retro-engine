// @file ButtonInput.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;

namespace RetroEngine.Interaction;

public enum ButtonInputType : byte
{
    LogicalKey,
    PhysicalKey,
    MouseButton,
    GamepadButton,
    AnyGamepadButton,
}

public readonly record struct GamepadButtonInput(byte Index, GamepadButton Button);

public readonly record struct AnyGamepadButtonInput(GamepadButton Button);

[StructLayout(LayoutKind.Sequential)]
public readonly struct ButtonInput : IEquatable<ButtonInput>, IEqualityOperators<ButtonInput, ButtonInput, bool>
{
    public ButtonInputType Type { get; }

    [StructLayout(LayoutKind.Explicit)]
    private readonly record struct ButtonsUnion
    {
        [field: FieldOffset(0)]
        public LogicalKey LogicalKey { get; init; }

        [field: FieldOffset(0)]
        public PhysicalKey PhysicalKey { get; init; }

        [field: FieldOffset(0)]
        public MouseButton MouseButton { get; init; }

        [field: FieldOffset(0)]
        public GamepadButtonInput GamepadButton { get; init; }

        [field: FieldOffset(0)]
        public AnyGamepadButtonInput AnyGamepadButton { get; init; }
    }

    private readonly ButtonsUnion _button;

    public object Button
    {
        get
        {
            return Type switch
            {
                ButtonInputType.LogicalKey => _button.LogicalKey,
                ButtonInputType.PhysicalKey => _button.PhysicalKey,
                ButtonInputType.MouseButton => _button.MouseButton,
                ButtonInputType.GamepadButton => _button.GamepadButton,
                ButtonInputType.AnyGamepadButton => _button.AnyGamepadButton,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public ButtonInput(LogicalKey logicalKey)
    {
        Type = ButtonInputType.LogicalKey;
        _button = new ButtonsUnion { LogicalKey = logicalKey };
    }

    public ButtonInput(PhysicalKey physicalKey)
    {
        Type = ButtonInputType.PhysicalKey;
        _button = new ButtonsUnion { PhysicalKey = physicalKey };
    }

    public ButtonInput(MouseButton mouseButton)
    {
        Type = ButtonInputType.MouseButton;
        _button = new ButtonsUnion { MouseButton = mouseButton };
    }

    public ButtonInput(GamepadButtonInput gamepadButton)
    {
        Type = ButtonInputType.GamepadButton;
        _button = new ButtonsUnion { GamepadButton = gamepadButton };
    }

    public ButtonInput(AnyGamepadButtonInput anyGamepadButton)
    {
        Type = ButtonInputType.AnyGamepadButton;
        _button = new ButtonsUnion { AnyGamepadButton = anyGamepadButton };
    }

    public static implicit operator ButtonInput(LogicalKey logicalKey) => new(logicalKey);

    public static implicit operator ButtonInput(PhysicalKey physicalKey) => new(physicalKey);

    public static implicit operator ButtonInput(MouseButton mouseButton) => new(mouseButton);

    public static implicit operator ButtonInput(GamepadButtonInput gamepadButton) => new(gamepadButton);

    public static implicit operator ButtonInput(AnyGamepadButtonInput anyGamepadButton) => new(anyGamepadButton);

    public bool TryGetValue(out LogicalKey logicalKey)
    {
        if (Type != ButtonInputType.LogicalKey)
        {
            logicalKey = default;
            return false;
        }

        logicalKey = _button.LogicalKey;
        return true;
    }

    public bool TryGetValue(out PhysicalKey physicalKey)
    {
        if (Type != ButtonInputType.PhysicalKey)
        {
            physicalKey = default;
            return false;
        }

        physicalKey = _button.PhysicalKey;
        return true;
    }

    public bool TryGetValue(out MouseButton mouseButton)
    {
        if (Type != ButtonInputType.MouseButton)
        {
            mouseButton = default;
            return false;
        }

        mouseButton = _button.MouseButton;
        return true;
    }

    public bool TryGetValue(out GamepadButtonInput gamepadButton)
    {
        if (Type != ButtonInputType.GamepadButton)
        {
            gamepadButton = default;
            return false;
        }

        gamepadButton = _button.GamepadButton;
        return true;
    }

    public bool TryGetValue(out AnyGamepadButtonInput anyGamepadButtonInput)
    {
        if (Type != ButtonInputType.AnyGamepadButton)
        {
            anyGamepadButtonInput = default;
            return false;
        }

        anyGamepadButtonInput = _button.AnyGamepadButton;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is ButtonInput other && Equals(other);
    }

    public bool Equals(ButtonInput other)
    {
        if (Type != other.Type)
            return false;

        return Type switch
        {
            ButtonInputType.LogicalKey => _button.LogicalKey == other._button.LogicalKey,
            ButtonInputType.PhysicalKey => _button.PhysicalKey == other._button.PhysicalKey,
            ButtonInputType.MouseButton => _button.MouseButton == other._button.MouseButton,
            ButtonInputType.GamepadButton => _button.GamepadButton == other._button.GamepadButton,
            ButtonInputType.AnyGamepadButton => _button.AnyGamepadButton == other._button.AnyGamepadButton,
            _ => false,
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            ButtonInputType.LogicalKey => HashCode.Combine(Type, _button.LogicalKey),
            ButtonInputType.PhysicalKey => HashCode.Combine(Type, _button.PhysicalKey),
            ButtonInputType.MouseButton => HashCode.Combine(Type, _button.MouseButton),
            ButtonInputType.GamepadButton => HashCode.Combine(Type, _button.GamepadButton),
            ButtonInputType.AnyGamepadButton => HashCode.Combine(Type, _button.AnyGamepadButton),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static bool operator ==(ButtonInput left, ButtonInput right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ButtonInput left, ButtonInput right)
    {
        return !left.Equals(right);
    }
}
