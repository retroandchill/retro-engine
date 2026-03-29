// // @file Text.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
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

    private static readonly ITextData EmptyTextData = new TextHistorySimple();

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

    internal Text(string sourceString, TextKey @namespace, TextKey key, TextFlag flags = TextFlag.None)
    {
        TextData = new TextHistorySimple(new TextId(@namespace, key), sourceString);
        Flags = flags;
    }

    public Text(string sourceString)
        : this(new TextHistorySimple(TextId.Empty, sourceString), TextFlag.InitializedFromString) { }

    public static implicit operator Text(string? sourceString) => new(sourceString ?? string.Empty);

    public static implicit operator string(Text text) => text.ToString();

    public static Text AsNumber(
        FormatNumericArg value,
        NumberFormattingOptions? options = null,
        Culture? targetCulture = null
    )
    {
        return new Text(new TextHistoryAsNumber(value, options, targetCulture), TextFlag.Transient);
    }

    public static Text AsPercent(
        FormatNumericArg value,
        NumberFormattingOptions? options = null,
        Culture? targetCulture = null
    )
    {
        return new Text(new TextHistoryAsPercent(value, options, targetCulture), TextFlag.Transient);
    }

    public static Text AsCurrency(
        FormatNumericArg value,
        string? currencyCode = null,
        NumberFormattingOptions? options = null,
        Culture? targetCulture = null
    )
    {
        return new Text(new TextHistoryAsCurrency(value, currencyCode, options, targetCulture), TextFlag.Transient);
    }

    public static Text AsDate(
        DateTimeOffset dateTime,
        DateTimeFormatStyle format = DateTimeFormatStyle.Default,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        return new Text(new TextHistoryAsDate(dateTime, format, timeZoneId, targetCulture), TextFlag.Transient);
    }

    public static Text AsTime(
        DateTimeOffset dateTime,
        DateTimeFormatStyle format = DateTimeFormatStyle.Default,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        return new Text(new TextHistoryAsTime(dateTime, format, timeZoneId, targetCulture), TextFlag.Transient);
    }

    public static Text AsDateTime(
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormat = DateTimeFormatStyle.Default,
        DateTimeFormatStyle timeFormat = DateTimeFormatStyle.Default,
        string? timeZoneId = null,
        Culture? targetCulture = null
    )
    {
        var culture = targetCulture ?? CultureManager.Instance.CurrentLocale;
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
        var culture = targetCulture ?? CultureManager.Instance.CurrentLocale;
        var nativeString = TextChronoFormatter.AsDateTime(dateTime, pattern, timeZoneId, culture);
        return new Text(
            new TextHistoryAsDateTime(nativeString, dateTime, pattern, timeZoneId, targetCulture),
            TextFlag.Transient
        );
    }

    public static Text AsTimespan(TimeSpan timeSpan, Culture? targetCulture = null)
    {
        var culture = targetCulture ?? CultureManager.Instance.CurrentLocale;

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

    public const string InvariantTimeZone = "Etc/Unknown";

    public static Text FromName(Name name)
    {
        return new Text(name.ToString());
    }

    public static Text AsCultureInvariant(string sourceString)
    {
        return new Text(new TextHistorySimple(TextId.Empty, sourceString), TextFlag.CultureInvariant);
    }

    public Text AsCultureInvariant()
    {
        return this with { Flags = Flags | TextFlag.CultureInvariant };
    }

    public static Text AsLocalizable(TextKey @namespace, TextKey key, string str)
    {
        return !string.IsNullOrEmpty(str) ? TextCache.Instance.FindOrCache(str, new TextId(@namespace, key)) : Empty;
    }

    public static Text AsLocalizable(TextKey @namespace, TextKey key, ReadOnlySpan<char> str)
    {
        return !str.IsEmpty ? TextCache.Instance.FindOrCache(str, new TextId(@namespace, key)) : Empty;
    }

    public static Text? FindTextInLiveTable(TextKey @namespace, TextKey key, string? sourceString = null)
    {
        var foundString = LocalizationManager.Instance.FindDisplayString(@namespace, key, sourceString);
        return foundString is not null
            ? new Text(new TextHistorySimple(new TextId(@namespace, key), foundString))
            : null;
    }

    public static IEnumerable<string> GetFormatPatternParameters(TextFormat format)
    {
        return format.FormatArgumentNames;
    }

    public static Text Format(TextFormat format, IReadOnlyDictionary<string, FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments);
    }

    public static Text Format(TextFormat format, ImmutableDictionary<string, FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments);
    }

    public static Text Format(TextFormat format, IReadOnlyList<FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments);
    }

    public static Text Format(TextFormat format, ImmutableArray<FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments);
    }

    [OverloadResolutionPriority(1)]
    public static Text Format(TextFormat format, params ReadOnlySpan<FormatArg> arguments)
    {
        return TextFormatter.Format(format, arguments);
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
        return TextFormatter.Format(namedFormatPattern, namedArgs);
    }

    public Text ToLower()
    {
        return new Text(
            new TextHistoryTransformed(this, TextHistoryTransformed.TransformType.ToLower),
            TextFlag.Transient
        );
    }

    public Text ToUpper()
    {
        return new Text(
            new TextHistoryTransformed(this, TextHistoryTransformed.TransformType.ToUpper),
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
        Rebuild();
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
        TextData.History.UpdateDisplayStringIfOutOfDate();
    }
}
