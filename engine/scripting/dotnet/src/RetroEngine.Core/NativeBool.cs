// @file $NativeBool.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using RetroEngine.Binds;

namespace RetroEngine.Core;

[BlittableType("bool")]
public enum NativeBool : byte
{
    False = 0,
    True = 1,
}

public static class BoolConverter
{
    public static NativeBool ToNativeBool(this bool value)
    {
        return value ? NativeBool.True : NativeBool.False;
    }

    public static bool ToManagedBool(this NativeBool value)
    {
        var byteValue = (byte)value;
        return byteValue != 0;
    }
}
