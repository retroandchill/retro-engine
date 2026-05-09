// // @file GuardValue.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities;

public readonly ref struct GuardValue<T> : IDisposable
{
    private readonly ref T _refValue;

    public T OriginalValue { get; }

    public GuardValue(ref T value, in T newValue)
    {
        _refValue = ref value;
        OriginalValue = value;
        _refValue = newValue;
    }

    public void Dispose()
    {
        _refValue = OriginalValue;
    }
}

public static class GuardValue
{
    public static GuardValue<T> Create<T>(ref T value, in T newValue)
    {
        return new GuardValue<T>(ref value, newValue);
    }
}
