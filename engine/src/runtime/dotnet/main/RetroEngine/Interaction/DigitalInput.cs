// @file ButtonInput.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RetroEngine.Interaction;

public enum DigitalInputType : byte
{
    LogicalKey,
    PhysicalKey,
    MouseButton,
    GamepadButton,
    AnyGamepadButton,
    GamepadAxisThreshold,
    AnyGamepadAxisThreshold,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct GamepadButtonInput(byte Index, GamepadButton Button);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct AnyGamepadButtonInput(GamepadButton Button);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct GamepadAxisThreshold(byte Index, GamepadAxis Axis, float Threshold);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct AnyGamepadAxisThreshold(GamepadAxis Axis, float Threshold);

[StructLayout(LayoutKind.Sequential)]
public readonly struct DigitalInput : IEquatable<DigitalInput>, IEqualityOperators<DigitalInput, DigitalInput, bool>
{
    public DigitalInputType Type { get; }

    [StructLayout(LayoutKind.Explicit)]
    private readonly record struct Union
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

        [field: FieldOffset(0)]
        public GamepadAxisThreshold GamepadAxisThreshold { get; init; }

        [field: FieldOffset(0)]
        public AnyGamepadAxisThreshold AnyGamepadAxisThreshold { get; init; }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Union _button;

    public object Button
    {
        get
        {
            return Type switch
            {
                DigitalInputType.LogicalKey => _button.LogicalKey,
                DigitalInputType.PhysicalKey => _button.PhysicalKey,
                DigitalInputType.MouseButton => _button.MouseButton,
                DigitalInputType.GamepadButton => _button.GamepadButton,
                DigitalInputType.AnyGamepadButton => _button.AnyGamepadButton,
                DigitalInputType.GamepadAxisThreshold => _button.GamepadAxisThreshold,
                DigitalInputType.AnyGamepadAxisThreshold => _button.AnyGamepadAxisThreshold,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public DigitalInput(LogicalKey logicalKey)
    {
        Type = DigitalInputType.LogicalKey;
        _button = new Union { LogicalKey = logicalKey };
    }

    public DigitalInput(PhysicalKey physicalKey)
    {
        Type = DigitalInputType.PhysicalKey;
        _button = new Union { PhysicalKey = physicalKey };
    }

    public DigitalInput(MouseButton mouseButton)
    {
        Type = DigitalInputType.MouseButton;
        _button = new Union { MouseButton = mouseButton };
    }

    public DigitalInput(GamepadButtonInput gamepadButton)
    {
        Type = DigitalInputType.GamepadButton;
        _button = new Union { GamepadButton = gamepadButton };
    }

    public DigitalInput(AnyGamepadButtonInput anyGamepadButton)
    {
        Type = DigitalInputType.AnyGamepadButton;
        _button = new Union { AnyGamepadButton = anyGamepadButton };
    }

    public DigitalInput(GamepadAxisThreshold gamepadAxisThreshold)
    {
        Type = DigitalInputType.GamepadAxisThreshold;
        _button = new Union { GamepadAxisThreshold = gamepadAxisThreshold };
    }

    public DigitalInput(AnyGamepadAxisThreshold anyGamepadAxisThreshold)
    {
        Type = DigitalInputType.AnyGamepadAxisThreshold;
        _button = new Union { AnyGamepadAxisThreshold = anyGamepadAxisThreshold };
    }

    public static implicit operator DigitalInput(LogicalKey logicalKey) => new(logicalKey);

    public static implicit operator DigitalInput(PhysicalKey physicalKey) => new(physicalKey);

    public static implicit operator DigitalInput(MouseButton mouseButton) => new(mouseButton);

    public static implicit operator DigitalInput(GamepadButtonInput gamepadButton) => new(gamepadButton);

    public static implicit operator DigitalInput(AnyGamepadButtonInput anyGamepadButton) => new(anyGamepadButton);

    public static implicit operator DigitalInput(GamepadAxisThreshold gamepadAxisThreshold) =>
        new(gamepadAxisThreshold);

    public static implicit operator DigitalInput(AnyGamepadAxisThreshold anyGamepadAxisThreshold) =>
        new(anyGamepadAxisThreshold);

    public bool TryGetValue(out LogicalKey logicalKey)
    {
        if (Type != DigitalInputType.LogicalKey)
        {
            logicalKey = default;
            return false;
        }

        logicalKey = _button.LogicalKey;
        return true;
    }

    public bool TryGetValue(out PhysicalKey physicalKey)
    {
        if (Type != DigitalInputType.PhysicalKey)
        {
            physicalKey = default;
            return false;
        }

        physicalKey = _button.PhysicalKey;
        return true;
    }

    public bool TryGetValue(out MouseButton mouseButton)
    {
        if (Type != DigitalInputType.MouseButton)
        {
            mouseButton = default;
            return false;
        }

        mouseButton = _button.MouseButton;
        return true;
    }

    public bool TryGetValue(out GamepadButtonInput gamepadButton)
    {
        if (Type != DigitalInputType.GamepadButton)
        {
            gamepadButton = default;
            return false;
        }

        gamepadButton = _button.GamepadButton;
        return true;
    }

    public bool TryGetValue(out AnyGamepadButtonInput anyGamepadButtonInput)
    {
        if (Type != DigitalInputType.AnyGamepadButton)
        {
            anyGamepadButtonInput = default;
            return false;
        }

        anyGamepadButtonInput = _button.AnyGamepadButton;
        return true;
    }

    public bool TryGetValue(out GamepadAxisThreshold gamepadAxisThreshold)
    {
        if (Type != DigitalInputType.GamepadAxisThreshold)
        {
            gamepadAxisThreshold = default;
            return false;
        }

        gamepadAxisThreshold = _button.GamepadAxisThreshold;
        return true;
    }

    public bool TryGetValue(out AnyGamepadAxisThreshold anyGamepadAxisThreshold)
    {
        if (Type != DigitalInputType.AnyGamepadAxisThreshold)
        {
            anyGamepadAxisThreshold = default;
            return false;
        }

        anyGamepadAxisThreshold = _button.AnyGamepadAxisThreshold;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is DigitalInput other && Equals(other);
    }

    public bool Equals(DigitalInput other)
    {
        if (Type != other.Type)
            return false;

        return Type switch
        {
            DigitalInputType.LogicalKey => _button.LogicalKey == other._button.LogicalKey,
            DigitalInputType.PhysicalKey => _button.PhysicalKey == other._button.PhysicalKey,
            DigitalInputType.MouseButton => _button.MouseButton == other._button.MouseButton,
            DigitalInputType.GamepadButton => _button.GamepadButton == other._button.GamepadButton,
            DigitalInputType.AnyGamepadButton => _button.AnyGamepadButton == other._button.AnyGamepadButton,
            DigitalInputType.GamepadAxisThreshold => _button.GamepadAxisThreshold == other._button.GamepadAxisThreshold,
            DigitalInputType.AnyGamepadAxisThreshold => _button.AnyGamepadAxisThreshold
                == other._button.AnyGamepadAxisThreshold,
            _ => false,
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            DigitalInputType.LogicalKey => HashCode.Combine(Type, _button.LogicalKey),
            DigitalInputType.PhysicalKey => HashCode.Combine(Type, _button.PhysicalKey),
            DigitalInputType.MouseButton => HashCode.Combine(Type, _button.MouseButton),
            DigitalInputType.GamepadButton => HashCode.Combine(Type, _button.GamepadButton),
            DigitalInputType.AnyGamepadButton => HashCode.Combine(Type, _button.AnyGamepadButton),
            DigitalInputType.GamepadAxisThreshold => HashCode.Combine(Type, _button.GamepadAxisThreshold),
            DigitalInputType.AnyGamepadAxisThreshold => HashCode.Combine(Type, _button.AnyGamepadAxisThreshold),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            DigitalInputType.LogicalKey => $"{nameof(DigitalInputType.LogicalKey)}: {_button.LogicalKey}",
            DigitalInputType.PhysicalKey => $"{nameof(DigitalInputType.PhysicalKey)}: {_button.PhysicalKey}",
            DigitalInputType.MouseButton => $"{nameof(DigitalInputType.MouseButton)}: {_button.MouseButton}",
            DigitalInputType.GamepadButton => $"{nameof(DigitalInputType.GamepadButton)}: {_button.GamepadButton}",
            DigitalInputType.AnyGamepadButton =>
                $"{nameof(DigitalInputType.AnyGamepadButton)}: {_button.AnyGamepadButton}",
            DigitalInputType.GamepadAxisThreshold =>
                $"{nameof(DigitalInputType.GamepadAxisThreshold)}: {_button.GamepadAxisThreshold}",
            DigitalInputType.AnyGamepadAxisThreshold =>
                $"{nameof(DigitalInputType.AnyGamepadAxisThreshold)}: {_button.AnyGamepadAxisThreshold}",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static bool operator ==(DigitalInput left, DigitalInput right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DigitalInput left, DigitalInput right)
    {
        return !left.Equals(right);
    }
}
