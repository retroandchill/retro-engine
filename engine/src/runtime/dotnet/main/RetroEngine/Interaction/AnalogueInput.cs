// @file AnalogueInput.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RetroEngine.Interaction;

public enum AnalogueInputType : byte
{
    LogicalKey,
    PhysicalKey,
    MouseButton,
    GamepadButton,
    AnyGamepadButton,
    GamepadAxis,
    AnyGamepadAxis,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct LogicalKeyAnalogueInput(LogicalKey Key, float Value);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct PhysicalKeyAnalogueInput(PhysicalKey Key, float Value);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct MouseButtonAnalogueInput(MouseButton Button, float Value);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct GamepadButtonAnalogueInput(byte Index, GamepadButton Button, float Value);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct AnyGamepadButtonAnalogueInput(GamepadButton Button, float Value);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct GamepadAxisInput(byte Index, GamepadAxis Axis, bool Invert = false);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct AnyGamepadAxisInput(GamepadAxis Axis, bool Invert = false);

[StructLayout(LayoutKind.Sequential)]
public readonly struct AnalogueInput : IEquatable<AnalogueInput>, IEqualityOperators<AnalogueInput, AnalogueInput, bool>
{
    public AnalogueInputType Type { get; }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct Union
    {
        [field: FieldOffset(0)]
        public LogicalKeyAnalogueInput LogicalKey { get; init; }

        [field: FieldOffset(0)]
        public PhysicalKeyAnalogueInput PhysicalKey { get; init; }

        [field: FieldOffset(0)]
        public MouseButtonAnalogueInput MouseButton { get; init; }

        [field: FieldOffset(0)]
        public GamepadButtonAnalogueInput GamepadButton { get; init; }

        [field: FieldOffset(0)]
        public AnyGamepadButtonAnalogueInput AnyGamepadButton { get; init; }

        [field: FieldOffset(0)]
        public GamepadAxisInput GamepadAxis { get; init; }

        [field: FieldOffset(0)]
        public AnyGamepadAxisInput AnyGamepadAxis { get; init; }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Union _axis;

    public AnalogueInput(LogicalKeyAnalogueInput input)
    {
        Type = AnalogueInputType.LogicalKey;
        _axis = new Union { LogicalKey = input };
    }

    public AnalogueInput(PhysicalKeyAnalogueInput input)
    {
        Type = AnalogueInputType.PhysicalKey;
        _axis = new Union { PhysicalKey = input };
    }

    public AnalogueInput(MouseButtonAnalogueInput input)
    {
        Type = AnalogueInputType.MouseButton;
        _axis = new Union { MouseButton = input };
    }

    public AnalogueInput(GamepadButtonAnalogueInput input)
    {
        Type = AnalogueInputType.GamepadButton;
        _axis = new Union { GamepadButton = input };
    }

    public AnalogueInput(AnyGamepadButtonAnalogueInput input)
    {
        Type = AnalogueInputType.AnyGamepadButton;
        _axis = new Union { AnyGamepadButton = input };
    }

    public AnalogueInput(GamepadAxisInput input)
    {
        Type = AnalogueInputType.GamepadAxis;
        _axis = new Union { GamepadAxis = input };
    }

    public AnalogueInput(AnyGamepadAxisInput input)
    {
        Type = AnalogueInputType.AnyGamepadAxis;
        _axis = new Union { AnyGamepadAxis = input };
    }

    public static implicit operator AnalogueInput(LogicalKeyAnalogueInput input) => new(input);

    public static implicit operator AnalogueInput(PhysicalKeyAnalogueInput input) => new(input);

    public static implicit operator AnalogueInput(MouseButtonAnalogueInput input) => new(input);

    public static implicit operator AnalogueInput(GamepadButtonAnalogueInput input) => new(input);

    public static implicit operator AnalogueInput(AnyGamepadButtonAnalogueInput input) => new(input);

    public static implicit operator AnalogueInput(GamepadAxisInput input) => new(input);

    public static implicit operator AnalogueInput(AnyGamepadAxisInput input) => new(input);

    public bool TryGetValue(out LogicalKeyAnalogueInput input)
    {
        if (Type != AnalogueInputType.LogicalKey)
        {
            input = default;
            return false;
        }

        input = _axis.LogicalKey;
        return true;
    }

    public bool TryGetValue(out PhysicalKeyAnalogueInput input)
    {
        if (Type != AnalogueInputType.PhysicalKey)
        {
            input = default;
            return false;
        }

        input = _axis.PhysicalKey;
        return true;
    }

    public bool TryGetValue(out MouseButtonAnalogueInput input)
    {
        if (Type != AnalogueInputType.MouseButton)
        {
            input = default;
            return false;
        }

        input = _axis.MouseButton;
        return true;
    }

    public bool TryGetValue(out GamepadButtonAnalogueInput input)
    {
        if (Type != AnalogueInputType.GamepadButton)
        {
            input = default;
            return false;
        }

        input = _axis.GamepadButton;
        return true;
    }

    public bool TryGetValue(out AnyGamepadButtonAnalogueInput input)
    {
        if (Type != AnalogueInputType.AnyGamepadButton)
        {
            input = default;
            return false;
        }

        input = _axis.AnyGamepadButton;
        return true;
    }

    public bool TryGetValue(out GamepadAxisInput input)
    {
        if (Type != AnalogueInputType.GamepadAxis)
        {
            input = default;
            return false;
        }

        input = _axis.GamepadAxis;
        return true;
    }

    public bool TryGetValue(out AnyGamepadAxisInput input)
    {
        if (Type != AnalogueInputType.AnyGamepadAxis)
        {
            input = default;
            return false;
        }

        input = _axis.AnyGamepadAxis;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is AnalogueInput other && Equals(other);
    }

    public bool Equals(AnalogueInput other)
    {
        if (Type != other.Type)
            return false;

        return Type switch
        {
            AnalogueInputType.LogicalKey => _axis.LogicalKey == other._axis.LogicalKey,
            AnalogueInputType.PhysicalKey => _axis.PhysicalKey == other._axis.PhysicalKey,
            AnalogueInputType.MouseButton => _axis.MouseButton == other._axis.MouseButton,
            AnalogueInputType.GamepadButton => _axis.GamepadButton == other._axis.GamepadButton,
            AnalogueInputType.AnyGamepadButton => _axis.AnyGamepadButton == other._axis.AnyGamepadButton,
            AnalogueInputType.GamepadAxis => _axis.GamepadAxis == other._axis.GamepadAxis,
            AnalogueInputType.AnyGamepadAxis => _axis.AnyGamepadAxis == other._axis.AnyGamepadAxis,
            _ => throw new InvalidOperationException("Unknown analogue input type"),
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            AnalogueInputType.LogicalKey => HashCode.Combine(Type, _axis.LogicalKey),
            AnalogueInputType.PhysicalKey => HashCode.Combine(Type, _axis.PhysicalKey),
            AnalogueInputType.MouseButton => HashCode.Combine(Type, _axis.MouseButton),
            AnalogueInputType.GamepadButton => HashCode.Combine(Type, _axis.GamepadButton),
            AnalogueInputType.AnyGamepadButton => HashCode.Combine(Type, _axis.AnyGamepadButton),
            AnalogueInputType.GamepadAxis => HashCode.Combine(Type, _axis.GamepadAxis),
            AnalogueInputType.AnyGamepadAxis => HashCode.Combine(Type, _axis.AnyGamepadAxis),
            _ => throw new InvalidOperationException("Unknown analogue input type"),
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            AnalogueInputType.LogicalKey => $"{nameof(AnalogueInputType.LogicalKey)}: {_axis.LogicalKey}",
            AnalogueInputType.PhysicalKey => $"{nameof(AnalogueInputType.PhysicalKey)}: {_axis.PhysicalKey}",
            AnalogueInputType.MouseButton => $"{nameof(AnalogueInputType.MouseButton)}: {_axis.MouseButton}",
            AnalogueInputType.GamepadButton => $"{nameof(AnalogueInputType.GamepadButton)}: {_axis.GamepadButton}",
            AnalogueInputType.AnyGamepadButton =>
                $"{nameof(AnalogueInputType.AnyGamepadButton)}: {_axis.AnyGamepadButton}",
            AnalogueInputType.GamepadAxis => $"{nameof(AnalogueInputType.GamepadAxis)}: {_axis.GamepadAxis}",
            AnalogueInputType.AnyGamepadAxis => $"{nameof(AnalogueInputType.AnyGamepadAxis)}: {_axis.AnyGamepadAxis}",
            _ => throw new InvalidOperationException("Unknown analogue input type"),
        };
    }

    public static bool operator ==(AnalogueInput left, AnalogueInput right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnalogueInput left, AnalogueInput right)
    {
        return !left.Equals(right);
    }
}
