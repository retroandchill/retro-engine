// // @file TextKey.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TextKey : IEquatable<TextKey>, IComparable<TextKey>, IComparisonOperators<TextKey, TextKey, bool>
{
    public uint Id { get; }

    public bool IsEmpty => Id == 0;

    public static readonly TextKey Empty = new(0);

    public TextKey(ReadOnlySpan<char> key)
        : this(TextKeyRegistry.Instance.FindOrAdd(key)) { }

    internal TextKey(uint id)
    {
        Id = id;
    }

    public static implicit operator TextKey(string key) => new(key);

    public override string ToString()
    {
        return TextKeyRegistry.Instance.GetString(Id);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is TextKey other && Equals(other);
    }

    public bool Equals(TextKey other)
    {
        return this == other;
    }

    public int CompareTo(TextKey other)
    {
        return Id.CompareTo(other.Id);
    }

    public static bool operator ==(TextKey left, TextKey right)
    {
        return left.Id == right.Id;
    }

    public static bool operator !=(TextKey left, TextKey right)
    {
        return !(left == right);
    }

    public static bool operator >(TextKey left, TextKey right)
    {
        return left.Id > right.Id;
    }

    public static bool operator >=(TextKey left, TextKey right)
    {
        return left.Id >= right.Id;
    }

    public static bool operator <(TextKey left, TextKey right)
    {
        return left.Id < right.Id;
    }

    public static bool operator <=(TextKey left, TextKey right)
    {
        return left.Id <= right.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public readonly record struct TextId(TextKey Namespace, TextKey Key)
    : IComparable<TextId>,
        IComparisonOperators<TextId, TextId, bool>
{
    public static readonly TextId Empty = new(TextKey.Empty, TextKey.Empty);

    public bool IsEmpty => Namespace.IsEmpty && Key.IsEmpty;

    public int CompareTo(TextId other)
    {
        var namespaceComparison = Namespace.CompareTo(other.Namespace);
        return namespaceComparison != 0 ? namespaceComparison : Key.CompareTo(other.Key);
    }

    public static bool operator >(TextId left, TextId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(TextId left, TextId right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(TextId left, TextId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(TextId left, TextId right)
    {
        return left.CompareTo(right) <= 0;
    }
}
