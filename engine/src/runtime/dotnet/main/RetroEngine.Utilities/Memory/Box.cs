// // @file Box.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities.Memory;

public sealed class Box<T>(T value)
    where T : struct
{
    // ReSharper disable once ReplaceWithFieldKeyword
    private T _value = value;

    public ref T Value => ref _value;
}

public sealed class ReadOnlyBox<T>(T value)
    where T : struct
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly T _value = value;

    public ref readonly T Value => ref _value;
}

public static class Box
{
    public static Box<T> Create<T>(T value)
        where T : struct => new(value);

    public static ReadOnlyBox<T> CreateReadOnly<T>(T value)
        where T : struct => new(value);
}
