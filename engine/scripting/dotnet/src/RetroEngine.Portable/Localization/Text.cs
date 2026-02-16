// // @file Text.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Localization;

[Flags]
public enum TextFlag : uint
{
    None,
    Transient = 1 << 0,
    CultureInvariant = 1 << 1,
    ConvertedProperty = 1 << 2,
    Immutable = 1 << 3,
    InitializedFromString = 1 << 4,
}

[Flags]
public enum TextIdenticalModeFlags : byte
{
    None,
    DeepCompare = 1 << 0,
    LexicalCompareInvariants = 1 << 1,
}

public enum TextComparisonLevel
{
    Default,
    Primary,
    Secondary,
    Tertiary,
    Quaternary,
    Quinary,
}

public readonly struct Text : IEquatable<Text>, IComparable<Text>, IComparisonOperators<Text, Text, bool>
{
    private readonly ITextData? _textData;
    private TextFlag Flags { get; init; }

    public bool IsEmpty => string.IsNullOrEmpty(ToString());

    public bool IsEmptyOrWhiteSpace => string.IsNullOrWhiteSpace(ToString());

    public bool IsTransient => Flags.HasFlag(TextFlag.Transient);

    public bool IsCultureInvariant => Flags.HasFlag(TextFlag.CultureInvariant);

    public bool IsInitializedFromString => Flags.HasFlag(TextFlag.InitializedFromString);

    private Text(ITextData textData, TextFlag glags = TextFlag.None)
    {
        _textData = textData;
        Flags = glags;
    }

    public Text(string sourceString)
        : this(new TextHistoryBase(TextId.Empty, sourceString), TextFlag.InitializedFromString) { }

    public static Text FromName(Name name)
    {
        return new Text(name.ToString());
    }

    public static Text AsCultureInvariant(string sourceString)
    {
        return new Text(new TextHistoryBase(TextId.Empty, sourceString), TextFlag.CultureInvariant);
    }

    public static Text AsCultureInvariant(Text text)
    {
        return text with { Flags = text.Flags | TextFlag.CultureInvariant };
    }

    public Text ToLower()
    {
        throw new NotImplementedException();
    }

    public Text ToUpper()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return _textData?.DisplayString ?? "";
    }

    public override bool Equals(object? obj)
    {
        return obj is Text other && Equals(other);
    }

    public bool Equals(Text other)
    {
        return Equals(other, TextComparisonLevel.Default);
    }

    public bool Equals(Text other, TextComparisonLevel level)
    {
        return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public int CompareTo(Text other)
    {
        return Compare(other, TextComparisonLevel.Default);
    }

    public int Compare(Text other, TextComparisonLevel level)
    {
        return ToString().CompareTo(other.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(Text left, Text right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Text left, Text right)
    {
        return !(left == right);
    }

    public static bool operator >(Text left, Text right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Text left, Text right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(Text left, Text right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Text left, Text right)
    {
        return left.CompareTo(right) <= 0;
    }
}
