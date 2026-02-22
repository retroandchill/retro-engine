// // @file Culture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum IcuErrorCode;

public sealed class Culture
{
    internal const int KeywordAndValuesCapacity = 100;
    private const int MillisPerSecond = 1000;

    private readonly Locale _locale;
    public CultureInfo CultureInfo { get; }
    public string DisplayName { get; private set; }
    public string EnglishName { get; private set; }
    public uint LCID => _locale.LCID;
    public string Name { get; }
    public string NativeName { get; private set; }
    public string ThreeLetterISOLanguageName { get; }
    public string TwoLetterISOLanguageName { get; }
    public string NativeLanguage { get; private set; }
    public string Region { get; }
    public string NativeRegion { get; private set; }
    public string Script { get; }
    public string Variant { get; }
    public bool IsRightToLeft { get; }

    public IEnumerable<string> PrioritizedParentCultureNames =>
        GetPrioritizedParentCultureNames(TwoLetterISOLanguageName, Script, Region);

    public static Culture CurrentCulture { get; }

    public static Culture DefaultCulture { get; }

    public static Culture InvariantCulture { get; }

    private static readonly Locale InvariantLocale;

    static Culture()
    {
        InvariantGregorianCalendar = Calendar.Create();
        InvariantGregorianCalendar?.TimeZone = TimeZone.Unknown;
        InvariantLocale =
            Locale.Create("us-EN-POSIX")
            ?? new Locale()
            ?? throw new InvalidOperationException("Invariant locale is null.");
        InvariantCulture = new Culture(InvariantLocale);
        DefaultCulture = new Culture(Locale.Default);
        CurrentCulture = DefaultCulture;
    }

    internal Culture(Locale locale)
    {
        _locale = locale;
        CultureInfo = CultureInfo.GetCultureInfo(locale.Name.Replace('_', '-'));
        Name = _locale.Name;
        ThreeLetterISOLanguageName = _locale.ThreeLetterISOLanguageName;
        TwoLetterISOLanguageName = _locale.TwoLetterISOLanguageName;
        Region = _locale.Region;
        Script = _locale.Script;
        Variant = _locale.Variant;
        IsRightToLeft = _locale.IsRightToLeft;

        RefreshCultureDisplayNames([]);

        _cardinalPluralRules =
            PluralRules.Create(_locale, PluralType.Cardinal)
            ?? PluralRules.Create(InvariantLocale, PluralType.Cardinal)
            ?? throw new InvalidOperationException("Cardinal plural rules are null.");
        _ordinalPluralRules =
            PluralRules.Create(_locale, PluralType.Ordinal)
            ?? PluralRules.Create(InvariantLocale, PluralType.Ordinal)
            ?? throw new InvalidOperationException("Ordinal plural rules are null.");
    }

    internal Culture(string cultureId)
        : this(CreateLocale(cultureId)) { }

    private static Locale CreateLocale(string cultureId)
    {
        var locale = Locale.Create(cultureId) ?? throw new CultureNotFoundException($"Culture {cultureId} not found.");
        return !locale.IsBogus ? locale : InvariantLocale;
    }

    public static string CreateCultureName(string languageCode, string scriptCode, string regionCode)
    {
        if (!string.IsNullOrEmpty(scriptCode) && !string.IsNullOrEmpty(regionCode))
        {
            return $"{languageCode}-{scriptCode}-{regionCode}";
        }

        if (!string.IsNullOrEmpty(regionCode))
        {
            return $"{languageCode}-{regionCode}";
        }

        return !string.IsNullOrEmpty(scriptCode) ? $"{languageCode}-{scriptCode}" : languageCode;
    }

    public static IEnumerable<string> GetPrioritizedParentCultureNames(
        string languageCode,
        string scriptCode,
        string regionCode
    )
    {
        if (!string.IsNullOrEmpty(scriptCode) && !string.IsNullOrEmpty(regionCode))
        {
            yield return CreateCultureName(languageCode, scriptCode, regionCode);
        }

        if (!string.IsNullOrEmpty(regionCode))
        {
            yield return CreateCultureName(languageCode, "", regionCode);
        }

        if (!string.IsNullOrEmpty(scriptCode))
        {
            yield return CreateCultureName(languageCode, scriptCode, "");
        }

        yield return languageCode;
    }

    internal static string GetCanonicalName(string name, CultureManager cultureManager)
    {
        return CultureUtilities.GetCanonicalName(name, "en-US-POSIX", cultureManager);
    }

    private readonly Lock _decimalNumberFormattingRulesLock = new();
    public DecimalNumberFormattingRules DecimalNumberFormattingRules
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            var decimalFormatterForCulture =
                DecimalFormat.CreateInstance(_locale)
                ?? DecimalFormat.CreateInstance(InvariantLocale)
                ?? throw new InvalidOperationException("Invariant culture decimal formatter is null.");
            var newFormattingRules = ExtractNumberFormattingRules(decimalFormatterForCulture);

            using var scope = _decimalNumberFormattingRulesLock.EnterScope();
            field ??= newFormattingRules;
            return field;
        }
    }

    private readonly Lock _percentFormattingRulesLock = new();
    public DecimalNumberFormattingRules PercentNumberFormattingRules
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            var decimalFormatterForCulture =
                DecimalFormat.CreatePercentInstance(_locale)
                ?? DecimalFormat.CreatePercentInstance(InvariantLocale)
                ?? throw new InvalidOperationException("Invariant culture decimal formatter is null.");
            var newFormattingRules = ExtractNumberFormattingRules(decimalFormatterForCulture);

            using var scope = _percentFormattingRulesLock.EnterScope();
            field ??= newFormattingRules;
            return field;
        }
    }

    private readonly ConcurrentDictionary<string, DecimalNumberFormattingRules> _currencyFormattingRules = new();
    private readonly Lock _defaultCurrencyFormattingRulesLock = new();
    private DecimalNumberFormattingRules? _defaultCurrencyFormattingRules;

    public DecimalNumberFormattingRules GetCurrencyFormattingRules(string? currencyCode = null)
    {
        var useDefaultFormattingRules = string.IsNullOrWhiteSpace(currencyCode);

        if (useDefaultFormattingRules)
        {
            if (_defaultCurrencyFormattingRules is not null)
            {
                return _defaultCurrencyFormattingRules;
            }
        }
        else
        {
            if (_currencyFormattingRules.TryGetValue(currencyCode ?? "", out var formattingRules))
            {
                return formattingRules;
            }
        }

        var decimalFormatterForCulture =
            DecimalFormat.CreateCurrencyInstance(_locale)
            ?? DecimalFormat.CreateCurrencyInstance(InvariantLocale)
            ?? throw new InvalidOperationException("Invariant culture decimal formatter is null.");

        if (!useDefaultFormattingRules)
        {
            decimalFormatterForCulture.CurrencyCode = currencyCode;
        }

        var newCurrencyFormattingRules = ExtractNumberFormattingRules(decimalFormatterForCulture);
        newCurrencyFormattingRules = newCurrencyFormattingRules with
        {
            PositivePrefixString = FixPrefixFormat(newCurrencyFormattingRules.PositivePrefixString),
            PositiveSuffixString = FixSuffixFormat(newCurrencyFormattingRules.PositiveSuffixString),
            NegativePrefixString = FixPrefixFormat(newCurrencyFormattingRules.NegativePrefixString),
            NegativeSuffixString = FixSuffixFormat(newCurrencyFormattingRules.NegativeSuffixString),
        };

        if (!useDefaultFormattingRules)
            return _currencyFormattingRules.GetOrAdd(currencyCode ?? "", newCurrencyFormattingRules);

        using var scope = _defaultCurrencyFormattingRulesLock.EnterScope();
        _defaultCurrencyFormattingRules ??= newCurrencyFormattingRules;
        return _defaultCurrencyFormattingRules;
    }

    private static string FixPrefixFormat(string currentPrefix)
    {
        return
            currentPrefix
                is [.., { IsValidCurrencyCode: true }, { IsValidCurrencyCode: true }, { IsValidCurrencyCode: true }]
            ? $"{currentPrefix}\u00A0"
            : currentPrefix;
    }

    private static string FixSuffixFormat(string currentSuffix)
    {
        return
            currentSuffix
                is [{ IsValidCurrencyCode: true }, { IsValidCurrencyCode: true }, { IsValidCurrencyCode: true }, ..]
            ? $"\u00A0{currentSuffix}"
            : currentSuffix;
    }

    private static unsafe DecimalNumberFormattingRules ExtractNumberFormattingRules(DecimalFormat decimalFormat)
    {
        var nativeFormattingRules = decimalFormat.FormattingRules;

        var formattingOptions = new NumberFormattingOptions
        {
            UseGrouping = nativeFormattingRules.IsGroupingUsed != 0,
            RoundingMode = nativeFormattingRules.RoundingMode.ToRoundingMode(),
            MinimumIntegralDigits = nativeFormattingRules.MinimumIntegerDigits,
            MaximumIntegralDigits = nativeFormattingRules.MaximumIntegerDigits,
            MinimumFractionalDigits = nativeFormattingRules.MinimumFractionDigits,
            MaximumFractionalDigits = nativeFormattingRules.MaximumFractionDigits,
        };

        decimalFormat.IsGroupingUsed = true;

        var (positivePrefix, positiveSuffix, negativePrefix, negativeSuffix) = decimalFormat.PrefixAndSuffix;

        const int digitsLength = NativeDecimalDigits.DigitsCapacity;
        return new DecimalNumberFormattingRules
        {
            DefaultFormattingOptions = formattingOptions,
            NanString = nativeFormattingRules.NanString.ToString(),
            PositivePrefixString = positivePrefix,
            PositiveSuffixString = positiveSuffix,
            NegativePrefixString = negativePrefix,
            NegativeSuffixString = negativeSuffix,
            PlusString = nativeFormattingRules.PlusSign.ToString(),
            MinusString = nativeFormattingRules.MinusSign.ToString(),
            GroupingSeparatorChar = nativeFormattingRules.GroupingSeperator,
            DecimalSeparatorChar = nativeFormattingRules.DecimalSeparator,
            PrimaryGroupingSize = (byte)nativeFormattingRules.GroupingSize,
            SecondaryGroupingSize = (byte)nativeFormattingRules.SecondaryGroupingSize,
            MinimumGroupingDigits = (byte)nativeFormattingRules.MinimumGroupingDigits,
            Digits = new ReadOnlySpan<char>(nativeFormattingRules.Digits.Digits, digitsLength).ToArray(),
        };
    }

    private static readonly Lock InvariantGregorianCalendarLock = new();
    private static readonly Calendar? InvariantGregorianCalendar;

    private DateFormat? _dateFormat;
    private DateFormat? _timeFormat;
    private DateFormat? _dateTimeFormat;

    internal static double DateTimeOffsetToIcuDate(DateTimeOffset dateTimeOffset)
    {
        if (InvariantGregorianCalendar is null)
        {
            var unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();
            return (double)unixTimestamp / MillisPerSecond;
        }

        var utcTime = dateTimeOffset.UtcDateTime;
        using var scope = InvariantGregorianCalendarLock.EnterScope();
        InvariantGregorianCalendar.Set(
            utcTime.Year,
            utcTime.Month - 1,
            utcTime.Day,
            utcTime.Hour,
            utcTime.Minute,
            utcTime.Second
        );
        return InvariantGregorianCalendar.Time;
    }

    private DecimalFormat DateTimeDecimalFormat
    {
        get
        {
            if (field is not null)
                return field;

            var formatter =
                DecimalFormat.CreateInstance(_locale)
                ?? DecimalFormat.CreateInstance(InvariantLocale)
                ?? throw new InvalidOperationException("Invariant culture decimal formatter is null.");

            var decimalSymbols = DecimalNumberFormattingRules;
            var nativeDigits = new NativeDecimalDigits();
            unsafe
            {
                nativeDigits.Digits[0] = decimalSymbols.Digits[0];
                nativeDigits.Digits[1] = decimalSymbols.Digits[1];
                nativeDigits.Digits[2] = decimalSymbols.Digits[2];
                nativeDigits.Digits[3] = decimalSymbols.Digits[3];
                nativeDigits.Digits[4] = decimalSymbols.Digits[4];
                nativeDigits.Digits[5] = decimalSymbols.Digits[5];
                nativeDigits.Digits[6] = decimalSymbols.Digits[6];
                nativeDigits.Digits[7] = decimalSymbols.Digits[7];
                nativeDigits.Digits[8] = decimalSymbols.Digits[8];
                nativeDigits.Digits[9] = decimalSymbols.Digits[9];
            }

            formatter.Digits = nativeDigits;
            formatter.IsGroupingUsed = false;

            field = formatter;
            return field;
        }
    }

    internal DateFormat GetDateFormatter(DateTimeFormatStyle dateStyle, string? timeZone)
    {
        _dateFormat ??= CreateDateFormat(_locale, DateTimeDecimalFormat);

        var defaultFormatter = _dateFormat;
        var isDefaultTimeZone = string.IsNullOrWhiteSpace(timeZone);
        if (!isDefaultTimeZone)
        {
            var canonicalInputTimeZoneId = TimeZone.GetCanonicalId(timeZone);
            var defaultTimeZoneId = defaultFormatter.TimeZone.Id;
            var canonicalDefaultTimeZoneId = TimeZone.GetCanonicalId(defaultTimeZoneId);

            isDefaultTimeZone = canonicalInputTimeZoneId == canonicalDefaultTimeZoneId;
        }

        var isDefault = dateStyle == DateTimeFormatStyle.Default && isDefaultTimeZone;

        if (isDefault)
            return defaultFormatter;

        var formatter =
            DateFormat.CreateDate(_locale, dateStyle.ToIcuEnum())
            ?? DateFormat.CreateDate(InvariantLocale, dateStyle.ToIcuEnum())
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
        formatter.TimeZone = isDefaultTimeZone ? new TimeZone() : new TimeZone(timeZone);
        return formatter;
    }

    internal DateFormat GetTimeFormatter(DateTimeFormatStyle timeStyle, string? timeZone)
    {
        _timeFormat ??= CreateTimeFormat(_locale, DateTimeDecimalFormat);

        var defaultFormatter = _timeFormat;
        var isDefaultTimeZone = string.IsNullOrWhiteSpace(timeZone);
        if (!isDefaultTimeZone)
        {
            var canonicalInputTimeZoneId = TimeZone.GetCanonicalId(timeZone);
            var defaultTimeZoneId = defaultFormatter.TimeZone.Id;
            var canonicalDefaultTimeZoneId = TimeZone.GetCanonicalId(defaultTimeZoneId);

            isDefaultTimeZone = canonicalInputTimeZoneId == canonicalDefaultTimeZoneId;
        }

        var isDefault = timeStyle == DateTimeFormatStyle.Default && isDefaultTimeZone;

        if (isDefault)
            return defaultFormatter;

        var formatter =
            DateFormat.CreateTime(_locale, timeStyle.ToIcuEnum())
            ?? DateFormat.CreateTime(InvariantLocale, timeStyle.ToIcuEnum())
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
        formatter.TimeZone = isDefaultTimeZone ? new TimeZone() : new TimeZone(timeZone);
        formatter.NumberFormat = DateTimeDecimalFormat;
        return formatter;
    }

    internal DateFormat GetDateTimeFormatter(
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string? timeZone
    )
    {
        _dateTimeFormat ??= CreateDateTimeFormat(_locale, DateTimeDecimalFormat);

        var defaultFormatter = _dateTimeFormat;
        var isDefaultTimeZone = string.IsNullOrWhiteSpace(timeZone);
        if (!isDefaultTimeZone)
        {
            var canonicalInputTimeZoneId = TimeZone.GetCanonicalId(timeZone);
            var defaultTimeZoneId = defaultFormatter.TimeZone.Id;
            var canonicalDefaultTimeZoneId = TimeZone.GetCanonicalId(defaultTimeZoneId);

            isDefaultTimeZone = canonicalInputTimeZoneId == canonicalDefaultTimeZoneId;
        }

        var isDefault =
            dateStyle == DateTimeFormatStyle.Default && timeStyle == DateTimeFormatStyle.Default && isDefaultTimeZone;

        if (isDefault)
            return defaultFormatter;

        var formatter =
            DateFormat.CreateTime(_locale, timeStyle.ToIcuEnum())
            ?? DateFormat.CreateTime(InvariantLocale, timeStyle.ToIcuEnum())
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
        formatter.TimeZone = isDefaultTimeZone ? new TimeZone() : new TimeZone(timeZone);
        formatter.NumberFormat = DateTimeDecimalFormat;
        return formatter;
    }

    internal DateFormat GetDateTimeFormatter(ReadOnlySpan<char> pattern, string? timeZone)
    {
        var dateFormat = DateFormat.Create(_locale, pattern) ?? DateFormat.Create(InvariantLocale, pattern);

        if (dateFormat is null)
            return GetDateTimeFormatter(DateTimeFormatStyle.Default, DateTimeFormatStyle.Default, timeZone);

        dateFormat.TimeZone = string.IsNullOrEmpty(timeZone) ? new TimeZone() : new TimeZone(timeZone);
        dateFormat.NumberFormat = DateTimeDecimalFormat;
        return dateFormat;
    }

    private static DateFormat CreateDateFormat(Locale locale, DecimalFormat numberFormat)
    {
        var dateFormat =
            DateFormat.CreateDate(locale, DateFormatStyle.Default)
            ?? DateFormat.CreateDate(InvariantLocale, DateFormatStyle.Default)
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
        dateFormat.TimeZone = new TimeZone();
        dateFormat.NumberFormat = numberFormat;
        return dateFormat;
    }

    private static DateFormat CreateTimeFormat(Locale locale, DecimalFormat numberFormat)
    {
        var dateFormat =
            DateFormat.CreateTime(locale, DateFormatStyle.Default)
            ?? DateFormat.CreateTime(InvariantLocale, DateFormatStyle.Default)
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
        dateFormat.TimeZone = new TimeZone();
        dateFormat.NumberFormat = numberFormat;
        return dateFormat;
    }

    private static DateFormat CreateDateTimeFormat(Locale locale, DecimalFormat numberFormat)
    {
        var dateFormat =
            DateFormat.CreateDateTime(locale, DateFormatStyle.Default, DateFormatStyle.Default)
            ?? DateFormat.CreateDateTime(InvariantLocale, DateFormatStyle.Default, DateFormatStyle.Default)
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
        dateFormat.TimeZone = new TimeZone();
        dateFormat.NumberFormat = numberFormat;
        return dateFormat;
    }

    private readonly PluralRules _cardinalPluralRules;
    private readonly PluralRules _ordinalPluralRules;

    public TextPluralForm GetPluralForm<T>(T value, TextPluralType pluralType)
        where T : unmanaged, INumber<T>
    {
        var rules = pluralType switch
        {
            TextPluralType.Cardinal => _cardinalPluralRules,
            TextPluralType.Ordinal => _ordinalPluralRules,
            _ => throw new ArgumentOutOfRangeException(nameof(pluralType), pluralType, null),
        };
        var formTag = value switch
        {
            sbyte i8 => rules.Select(i8),
            short i16 => rules.Select(i16),
            int i32 => rules.Select(i32),
            long i64 => rules.Select(i64),
            byte u8 => rules.Select(u8),
            ushort u16 => rules.Select(u16),
            uint u32 => rules.Select(u32),
            ulong u64 => rules.Select(u64),
            float f32 => rules.Select(f32),
            double f64 => rules.Select(f64),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
        return GetPluralForm(formTag);
    }

    private static TextPluralForm GetPluralForm(string tag)
    {
        return tag switch
        {
            "zero" => TextPluralForm.Zero,
            "one" => TextPluralForm.One,
            "two" => TextPluralForm.Two,
            "few" => TextPluralForm.Few,
            "many" => TextPluralForm.Many,
            "other" => TextPluralForm.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null),
        };
    }

    private Collator? _collator;

    internal Collator GetCollator(TextComparisonLevel level)
    {
        _collator ??= CreateCollator(_locale);
        var isDefault = level == TextComparisonLevel.CultureDefault;
        if (isDefault)
            return _collator;

        var collator = _collator.Clone();
        collator.Strength = level.ToCollationStrength();
        return collator;
    }

    private static Collator CreateCollator(Locale locale)
    {
        return Collator.Create(locale)
            ?? Collator.Create(InvariantLocale)
            ?? throw new InvalidOperationException("Invariant culture collator is null.");
    }

    public void RefreshCultureDisplayNames(ReadOnlySpan<string> prioritizedDisplayCultureNames, bool fullRefresh = true)
    {
        DisplayName = ApplyCultureDisplayNameSubstitutes(prioritizedDisplayCultureNames, _locale.DisplayName);

        if (!fullRefresh)
            return;

        EnglishName = ApplyCultureDisplayNameSubstitutes(["en"], _locale.EnglishName);

        var prioritizedParentCultureName = PrioritizedParentCultureNames.ToArray();

        var displayLanguage = _locale.DisplayLanguage;
        var displayRegion = _locale.DisplayCountry;
        var displayScript = _locale.DisplayScript;
        var displayVariant = _locale.DisplayVariant;
        NativeName = ApplyCultureDisplayNameSubstitutes(prioritizedParentCultureName, _locale.NativeName);
        NativeLanguage = ApplyCultureDisplayNameSubstitutes(
            prioritizedParentCultureName,
            displayScript.Length > 0 ? $"{displayLanguage} ({displayScript})" : displayLanguage
        );
        NativeRegion = ApplyCultureDisplayNameSubstitutes(
            prioritizedParentCultureName,
            displayVariant.Length > 0 ? $"{displayRegion} ({displayVariant})" : displayRegion
        );
    }

    private readonly record struct DisplayNameSubstitute(string Culture, string OldString, string NewString);

    private static readonly List<DisplayNameSubstitute> DisplayNameSubstitutes = [];

    private static string ApplyCultureDisplayNameSubstitutes(
        ReadOnlySpan<string> prioritizedDisplayCultureNames,
        string displayName
    )
    {
        // TODO: We may need to load config data from somwhere, but that isn't set up yet

        foreach (var substitute in DisplayNameSubstitutes)
        {
            if (string.IsNullOrEmpty(substitute.Culture) || prioritizedDisplayCultureNames.Contains(substitute.Culture))
            {
                displayName = displayName.Replace(substitute.OldString, substitute.NewString);
            }
        }

        return displayName;
    }
}
