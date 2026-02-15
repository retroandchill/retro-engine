// // @file TextId.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace RetroEngine.Portable.Localization;

public readonly struct TextKey : IEquatable<TextKey>, IEqualityOperators<TextKey, TextKey, bool>
{
    private readonly int _index;

    public bool IsEmpty => _index == 0;

    public TextKey(string str)
        : this(str.AsSpan()) { }

    public TextKey(ReadOnlySpan<char> str)
    {
        if (str.IsEmpty)
            return;

        this = TextKeyState.State.FindOrAdd(str);
    }

    public override string ToString()
    {
        return _index != 0 ? TextKeyState.State.GetStringByIndex(_index) : string.Empty;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is TextKey other && Equals(other);
    }

    public bool Equals(TextKey other)
    {
        return this == other;
    }

    public static bool operator ==(TextKey left, TextKey right)
    {
        return left._index == right._index;
    }

    public static bool operator !=(TextKey left, TextKey right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return _index != 0 ? TextKeyState.State.GetHashByIndex(_index) : 0;
    }
}

public readonly record struct TextId(TextKey Namespace, TextKey Key) : IEqualityOperators<TextId, TextId, bool>
{
    public bool IsEmpty => Namespace.IsEmpty && Key.IsEmpty;
}
