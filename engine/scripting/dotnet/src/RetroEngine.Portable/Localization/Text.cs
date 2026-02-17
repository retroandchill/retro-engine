// // @file Text.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Localization.Formatting;
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
    private ITextData TextData => field ?? EmptyTextData;

    private TextFlag Flags { get; init; }

    private static readonly ITextData EmptyTextData = new TextHistoryBase();

    public static readonly Text Empty = new(EmptyTextData);

    public bool IsEmpty => string.IsNullOrEmpty(ToString());

    public bool IsEmptyOrWhiteSpace => string.IsNullOrWhiteSpace(ToString());

    public bool IsTransient => Flags.HasFlag(TextFlag.Transient);

    public bool IsCultureInvariant => Flags.HasFlag(TextFlag.CultureInvariant);

    public bool IsInitializedFromString => Flags.HasFlag(TextFlag.InitializedFromString);

    private Text(ITextData textData, TextFlag flags = TextFlag.None)
    {
        TextData = textData;
        Flags = flags;
    }

    public Text(string sourceString)
        : this(new TextHistoryBase(TextId.Empty, sourceString), TextFlag.InitializedFromString) { }

    private static Text AsNumberInternal<T>(T value, NumberFormattingOptions? options, CultureHandle? targetCulture)
        where T : unmanaged, INumber<T>
    {
        var culture = targetCulture ?? LocalizationManager.Instance.CurrentCulture;
        var formattingRules = culture.NumberFormattingRules;
        var formattingOptions = options ?? formattingRules.DefaultFormattingOptions;
        var nativeString = FastDecimalFormat.NumberToString(value, formattingRules, formattingOptions);
        return new Text(new TextHistoryAsNumber<T>(nativeString, value, formattingOptions, targetCulture));
    }

    public static Text AsNumber(
        float value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        double value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        sbyte value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        short value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(int value, NumberFormattingOptions? options = null, CultureHandle? targetCulture = null)
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        long value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        byte value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        ushort value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        uint value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text AsNumber(
        ulong value,
        NumberFormattingOptions? options = null,
        CultureHandle? targetCulture = null
    )
    {
        return AsNumberInternal(value, options, targetCulture);
    }

    public static Text FromName(Name name)
    {
        return new Text(name.ToString());
    }

    public static Text AsCultureInvariant(string sourceString)
    {
        return new Text(new TextHistoryBase(TextId.Empty, sourceString), TextFlag.CultureInvariant);
    }

    public Text AsCultureInvariant()
    {
        return this with { Flags = Flags | TextFlag.CultureInvariant };
    }

    public Text ToLower()
    {
        var resultString = TextTransformer.ToUpper(ToString());
        return new Text(
            new TextHistoryTransformed(resultString, this, TextHistoryTransformed.TransformType.ToLower),
            TextFlag.Transient
        );
    }

    public Text ToUpper()
    {
        var resultString = TextTransformer.ToUpper(ToString());
        return new Text(
            new TextHistoryTransformed(resultString, this, TextHistoryTransformed.TransformType.ToUpper),
            TextFlag.Transient
        );
    }

    public Text TrimStart()
    {
        var currentString = ToString();
        var trimmedString = currentString.TrimStart();
        return IsCultureInvariant ? AsCultureInvariant(trimmedString) : new Text(trimmedString);
    }

    public Text TrimEnd()
    {
        var currentString = ToString();
        var trimmedString = currentString.TrimEnd();
        return IsCultureInvariant ? AsCultureInvariant(trimmedString) : new Text(trimmedString);
    }

    public Text Trim()
    {
        var currentString = ToString();
        var trimmedString = currentString.Trim();
        return IsCultureInvariant ? AsCultureInvariant(trimmedString) : new Text(trimmedString);
    }

    public override string ToString()
    {
        return TextData.DisplayString;
    }

    public string BuildSourceString()
    {
        return TextData.History.BuildInvariantDisplayString();
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

    public bool IdenticalTo(Text other, TextIdenticalModeFlags flags = TextIdenticalModeFlags.None)
    {
        if (ReferenceEquals(TextData, other.TextData))
            return true;

        Rebuild();
        other.Rebuild();

        var displayString = TextData.LocalizedString;
        var otherDisplayString = other.TextData.LocalizedString;
        if (
            displayString is not null
            && otherDisplayString is not null
            && ReferenceEquals(displayString, otherDisplayString)
        )
        {
            return true;
        }

        if (flags.HasFlag(TextIdenticalModeFlags.DeepCompare))
        {
            var thisHistory = TextData.History;
            var otherHistory = other.TextData.History;

            if (thisHistory.IdenticalTo(otherHistory, flags))
            {
                return true;
            }
        }

        if (!flags.HasFlag(TextIdenticalModeFlags.LexicalCompareInvariants))
            return false;

        var thisIsInvariant = (Flags & (TextFlag.CultureInvariant | TextFlag.InitializedFromString)) != 0;
        var otherIsInvariant = (other.Flags & (TextFlag.CultureInvariant | TextFlag.InitializedFromString)) != 0;

        return thisIsInvariant
            && otherIsInvariant & ToString().Equals(other.ToString(), StringComparison.InvariantCultureIgnoreCase);
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

    internal void Rebuild()
    {
        TextData.History.UpdateDisplayString();
    }
}
