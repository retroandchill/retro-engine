// // @file Text.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.History;
using RetroEngine.Portable.Strings;
using ZLinq;

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
    /// <summary>
    /// Uses the current culture's default string comparison rules.
    /// </summary>
    CultureDefault,

    /// <summary>
    /// Compares base characters only (ignores case, accents, width, and kana type).
    /// Suitable for very fuzzy, user-friendly matching.
    /// </summary>
    IgnoreCaseAccentWidth,

    /// <summary>
    /// Compares letters and accents but ignores case (A == a, Á != A).
    /// </summary>
    IgnoreCase,

    /// <summary>
    /// Case- and accent-sensitive comparison using the current culture.
    /// </summary>
    CultureSensitive,

    /// <summary>
    /// Culture-sensitive comparison that treats punctuation and symbols
    /// as significant for sorting.
    /// </summary>
    CultureSensitiveWithPunctuation,

    /// <summary>
    /// Exact, ordinal (binary) comparison of the underlying characters.
    /// </summary>
    Ordinal,
}

public readonly struct Text : IEquatable<Text>, IComparable<Text>, IComparisonOperators<Text, Text, bool>
{
    internal ITextData TextData => field ?? EmptyTextData;

    internal TextFlag Flags { get; private init; }

    private static readonly ITextData EmptyTextData = new TextHistoryBase();

    public static readonly Text Empty = new(EmptyTextData);

    public bool IsEmpty => string.IsNullOrEmpty(ToString());

    public bool IsEmptyOrWhiteSpace => string.IsNullOrWhiteSpace(ToString());

    public bool IsTransient => Flags.HasFlag(TextFlag.Transient);

    public bool IsCultureInvariant => Flags.HasFlag(TextFlag.CultureInvariant);

    public bool IsInitializedFromString => Flags.HasFlag(TextFlag.InitializedFromString);

    internal Text(ITextData textData, TextFlag flags = TextFlag.None)
    {
        TextData = textData;
        Flags = flags;
    }

    public Text(string sourceString)
        : this(new TextHistoryBase(TextId.Empty, sourceString), TextFlag.InitializedFromString) { }

    public static implicit operator Text(string? sourceString) => new(sourceString ?? string.Empty);

    public static Text AsNumber<T>(T value, NumberFormattingOptions? options = null, Culture? targetCulture = null)
        where T : unmanaged, INumber<T>
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var numberFormattingRules = culture.DecimalNumberFormattingRules;
        var nativeString = FastDecimalFormat.NumberToString(
            value,
            numberFormattingRules,
            options ?? numberFormattingRules.DefaultFormattingOptions
        );
        return new Text(new TextHistoryAsNumber<T>(nativeString, value, options, targetCulture), TextFlag.Transient);
    }

    public static Text AsPercent<T>(T value, NumberFormattingOptions? options = null, Culture? targetCulture = null)
        where T : unmanaged, IFloatingPoint<T>
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var numberFormattingRules = culture.PercentNumberFormattingRules;
        var nativeString = FastDecimalFormat.NumberToString(
            value,
            numberFormattingRules,
            options ?? numberFormattingRules.DefaultFormattingOptions
        );
        return new Text(new TextHistoryAsPercent<T>(nativeString, value, options, targetCulture), TextFlag.Transient);
    }

    public static Text AsCurrency<T>(
        T value,
        string? currencyCode = null,
        NumberFormattingOptions? options = null,
        Culture? targetCulture = null
    )
        where T : unmanaged, INumber<T>
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var numberFormattingRules = culture.GetCurrencyFormattingRules(currencyCode);
        var nativeString = FastDecimalFormat.NumberToString(
            value,
            numberFormattingRules,
            options ?? numberFormattingRules.DefaultFormattingOptions
        );
        return new Text(
            new TextHistoryAsCurrency<T>(nativeString, value, currencyCode, options, targetCulture),
            TextFlag.Transient
        );
    }

    public static Text AsDate(
        DateTimeOffset dateTime,
        DateTimeFormatStyle format = DateTimeFormatStyle.Default,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var nativeString = TextChronoFormatter.AsDate(dateTime, format, timeZoneId, culture);
        return new Text(
            new TextHistoryAsDate(nativeString, dateTime, format, timeZoneId, targetCulture),
            TextFlag.Transient
        );
    }

    public static Text AsTime(
        DateTimeOffset dateTime,
        DateTimeFormatStyle format = DateTimeFormatStyle.Default,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var nativeString = TextChronoFormatter.AsTime(dateTime, format, timeZoneId, culture);
        return new Text(
            new TextHistoryAsTime(nativeString, dateTime, format, timeZoneId, targetCulture),
            TextFlag.Transient
        );
    }

    public static Text AsDateTime(
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormat = DateTimeFormatStyle.Default,
        DateTimeFormatStyle timeFormat = DateTimeFormatStyle.Default,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var nativeString = TextChronoFormatter.AsDateTime(dateTime, dateFormat, timeFormat, timeZoneId, culture);
        return new Text(
            new TextHistoryAsDateTime(nativeString, dateTime, dateFormat, timeFormat, timeZoneId, targetCulture),
            TextFlag.Transient
        );
    }

    public static Text AsDateTime(
        DateTimeOffset dateTime,
        string pattern,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        var culture = targetCulture ?? Culture.CurrentCulture;
        var nativeString = TextChronoFormatter.AsDateTime(dateTime, pattern, timeZoneId, culture);
        return new Text(
            new TextHistoryAsDateTime(nativeString, dateTime, pattern, timeZoneId, targetCulture),
            TextFlag.Transient
        );
    }

    public static Text AsTimespan(TimeSpan timeSpan, Culture? targetCulture = null)
    {
        var culture = targetCulture ?? Culture.CurrentCulture;

        var totalHours = timeSpan.TotalHours;
        var hours = (int)totalHours;
        var minutes = timeSpan.Minutes;
        var seconds = timeSpan.Seconds;

        var formattingOptions = new NumberFormattingOptions() { MinimumIntegralDigits = 2, MaximumIntegralDigits = 2 };

        if (hours > 0)
        {
            var timespanFormatPattern = AsLocalizable(
                "Timespan",
                "Format_HoursMinutesSeconds",
                "{Hours}:{Minutes}:{Seconds}"
            );
            var timeArguments = new Dictionary<string, FormatArg>
            {
                ["Hours"] = hours,
                ["Minutes"] = AsNumber(minutes, formattingOptions, culture),
                ["Seconds"] = AsNumber(seconds, formattingOptions, culture),
            };
            return Format(timespanFormatPattern, timeArguments);
        }
        else
        {
            var timespanFormatPattern = AsLocalizable("Timespan", "Format_MinutesSeconds", "{Minutes}:{Seconds}");
            var timeArguments = new Dictionary<string, FormatArg>
            {
                ["Minutes"] = minutes,
                ["Seconds"] = AsNumber(seconds, formattingOptions, culture),
            };
            return Format(timespanFormatPattern, timeArguments);
        }
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

    public static Text AsLocalizable(TextKey ns, TextKey key, ReadOnlySpan<char> str)
    {
        throw new NotImplementedException();
    }

    public static Text AsLocalizable(ReadOnlySpan<char> ns, ReadOnlySpan<char> key, ReadOnlySpan<char> str)
    {
        return AsLocalizable(new TextKey(ns), new TextKey(key), str);
    }

    public static bool FindTextInLiveTable(TextKey ns, TextKey key, out Text result, string? sourceString = null)
    {
        throw new NotImplementedException();
    }

    public static Text Format(TextFormat format, IReadOnlyDictionary<string, FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments, false, false);
    }

    public static Text Format(TextFormat format, ImmutableDictionary<string, FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments, false, false);
    }

    public static Text Format(TextFormat format, IReadOnlyList<FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments, false, false);
    }

    public static Text Format(TextFormat format, ImmutableArray<FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments, false, false);
    }

    [OverloadResolutionPriority(1)]
    public static Text Format(TextFormat format, params ReadOnlySpan<FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments, false, false);
    }

    public static Text Join(Text separator, IEnumerable<FormatArg> elements)
    {
        return Join(separator, elements.AsValueEnumerable());
    }

    public static Text Join(Text separator, params ReadOnlySpan<FormatArg> elements)
    {
        if (elements.Length == 1 && elements[0].TryGetTextData(out var textData))
        {
            return textData;
        }

        return Join(separator, elements.AsValueEnumerable());
    }

    private static Text Join<TEnumerator>(Text separator, ValueEnumerable<TEnumerator, FormatArg> elements)
        where TEnumerator : struct, IValueEnumerator<FormatArg>, allows ref struct
    {
        if (elements.TryGetNonEnumeratedCount(out var count) && count == 0)
            return Empty;

        var formatPattern = new StringBuilder();
        var namedArgs = new Dictionary<string, FormatArg>();

        if (elements.TryGetNonEnumeratedCount(out var argsCount))
        {
            namedArgs.EnsureCapacity(argsCount + 1);
        }

        namedArgs.Add("Delimiter", separator);

        foreach (var (i, element) in elements.Index())
        {
            if (i != 0)
                formatPattern.Append("{Delimiter}");

            namedArgs.Add(i.ToString(), element);

            formatPattern.Append('{').Append(i).Append('}');
        }

        var namedFormatPattern = AsCultureInvariant(formatPattern.ToString());
        return TextFormatter.Format(namedFormatPattern, namedArgs, false, false);
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
        return Equals(other, TextComparisonLevel.CultureDefault);
    }

    public bool Equals(Text other, TextComparisonLevel level)
    {
        return TextComparison.Equals(ToString(), other.ToString(), level);
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

    private static CompareOptions GetCompareOptions(TextComparisonLevel level)
    {
        return level switch
        {
            TextComparisonLevel.CultureDefault => CompareOptions.None,
            TextComparisonLevel.IgnoreCaseAccentWidth => CompareOptions.IgnoreCase
                | CompareOptions.IgnoreNonSpace
                | CompareOptions.IgnoreKanaType
                | CompareOptions.IgnoreWidth,
            TextComparisonLevel.IgnoreCase => CompareOptions.IgnoreCase
                | CompareOptions.IgnoreKanaType
                | CompareOptions.IgnoreWidth,
            TextComparisonLevel.CultureSensitive => CompareOptions.None,
            TextComparisonLevel.CultureSensitiveWithPunctuation => CompareOptions.StringSort,
            TextComparisonLevel.Ordinal => CompareOptions.Ordinal,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public int CompareTo(Text other)
    {
        return Compare(other, TextComparisonLevel.CultureDefault);
    }

    public int Compare(Text other, TextComparisonLevel level)
    {
        return TextComparison.Compare(ToString(), other.ToString(), level);
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

    internal IEnumerable<HistoricTextFormatData> HistoricFormatData => TextData.History.GetHistoricFormatData(this);

    internal HistoricTextNumericData? HistoricNumericData => TextData.History.GetHistoricNumericData(this);

    internal void Rebuild()
    {
        TextData.History.UpdateDisplayString();
    }
}
