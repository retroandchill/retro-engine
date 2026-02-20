// // @file Culture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum IcuErrorCode;

public sealed partial class Culture
{
    internal const int KeywordAndValuesCapacity = 100;
    private static readonly CultureId EnglishCultureId = new("en");
    private const int MillisPerSecond = 1000;

    private readonly Locale _locale;
    public CultureId Id => _locale.Id;
    public CultureInfo CultureInfo { get; }
    public string DisplayName { get; }
    public string EnglishName { get; }
    public int LCID => NativeGetLCID(_locale.Id);
    public string Name { get; }
    public string ThreeLetterISOLanguageName { get; }
    public string TwoLetterISOLanguageName { get; }
    public string NativeLanguage { get; }
    public string Region { get; }
    public string NativeRegion { get; }
    public string Script { get; }
    public string Variant { get; }
    public bool IsRightToLeft { get; }

    public static Culture CurrentCulture { get; }

    public static Culture InvariantCulture { get; }

    private static readonly Locale InvariantLocale;

    static Culture()
    {
        InvariantLocale =
            Locale.Create(new CultureId("us-EN-POSIX"))
            ?? Locale.Create(new CultureId(""))
            ?? throw new InvalidOperationException("Invariant locale is null.");
        InvariantCulture = new Culture(InvariantLocale);
        CurrentCulture = new Culture(new CultureId(NativeGetDefault()));
    }

    private Culture(Locale locale)
    {
        _locale = locale;
        CultureInfo = CultureInfo.GetCultureInfo(locale.Id.ToString());
        Span<char> buffer = stackalloc char[KeywordAndValuesCapacity];
        Name = GetNativeString(locale.Id, buffer, NativeGetName);
        ThreeLetterISOLanguageName = NativeGetISO3Language(locale.Id);
        TwoLetterISOLanguageName = GetNativeString(locale.Id, buffer, NativeGetLanguage);
        Region = GetNativeString(locale.Id, buffer, NativeGetCountry);
        Script = GetNativeString(locale.Id, buffer, NativeGetScript);
        Variant = GetNativeString(locale.Id, buffer, NativeGetVariant);
        IsRightToLeft = NativeIsRightToLeft(locale.Id);

        DisplayName = GetNativeString(locale.Id, buffer, NativeGetDisplayName);
        EnglishName = GetNativeString(locale.Id, EnglishCultureId, buffer, NativeGetDisplayName);

        var displayLanguage = GetNativeString(locale.Id, buffer, NativeGetDisplayLanguage);
        var displayRegion = GetNativeString(locale.Id, buffer, NativeGetDisplayCountry);
        var displayScript = GetNativeString(locale.Id, buffer, NativeGetDisplayScript);
        var displayVariant = GetNativeString(locale.Id, buffer, NativeGetDisplayVariant);
        NativeLanguage = displayScript.Length > 0 ? $"{displayLanguage} ({displayScript})" : displayLanguage;
        NativeRegion = displayVariant.Length > 0 ? $"{displayRegion} ({displayVariant})" : displayRegion;

        _cardinalPluralRules =
            PluralRules.Create(locale.Id, PluralType.Cardinal)
            ?? PluralRules.Create(InvariantLocale.Id, PluralType.Cardinal)
            ?? throw new InvalidOperationException("Cardinal plural rules are null.");
        _ordinalPluralRules =
            PluralRules.Create(locale.Id, PluralType.Ordinal)
            ?? PluralRules.Create(InvariantLocale.Id, PluralType.Ordinal)
            ?? throw new InvalidOperationException("Ordinal plural rules are null.");
    }

    internal Culture(CultureId cultureId)
        : this(CreateLocale(cultureId)) { }

    private static Locale CreateLocale(CultureId cultureId)
    {
        var locale = Locale.Create(cultureId) ?? throw new CultureNotFoundException($"Culture {cultureId} not found.");
        return !locale.IsBogus ? locale : InvariantLocale;
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
                DecimalFormat.CreateInstance(_locale.Id)
                ?? DecimalFormat.CreateInstance(InvariantLocale.Id)
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
                DecimalFormat.CreatePercentInstance(_locale.Id)
                ?? DecimalFormat.CreatePercentInstance(InvariantLocale.Id)
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
            DecimalFormat.CreateCurrencyInstance(_locale.Id)
            ?? DecimalFormat.CreateCurrencyInstance(InvariantLocale.Id)
            ?? throw new InvalidOperationException("Invariant culture decimal formatter is null.");

        if (!useDefaultFormattingRules)
        {
            decimalFormatterForCulture.SetTextAttribute(NumberFormatTextAttribute.CurrencyCode, currencyCode);
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

    private static DecimalNumberFormattingRules ExtractNumberFormattingRules(DecimalFormat decimalFormat)
    {
        var formattingOptions = new NumberFormattingOptions
        {
            UseGrouping = decimalFormat.IsGroupingUsed,
            RoundingMode = decimalFormat.RoundingMode.ToRoundingMode(),
            MinimumIntegralDigits = decimalFormat.MinimumIntegerDigits,
            MaximumIntegralDigits = decimalFormat.MaximumIntegerDigits,
            MinimumFractionalDigits = decimalFormat.MinimumFractionDigits,
            MaximumFractionalDigits = decimalFormat.MaximumFractionDigits,
        };

        decimalFormat.IsGroupingUsed = true;

        return new DecimalNumberFormattingRules
        {
            DefaultFormattingOptions = formattingOptions,
            NanString = decimalFormat.GetSymbol(NumberFormatSymbol.NanSymbol),
            NegativePrefixString = decimalFormat.GetTextAttribute(NumberFormatTextAttribute.NegativePrefix),
            NegativeSuffixString = decimalFormat.GetTextAttribute(NumberFormatTextAttribute.NegativeSuffix),
            PositivePrefixString = decimalFormat.GetTextAttribute(NumberFormatTextAttribute.PositivePrefix),
            PositiveSuffixString = decimalFormat.GetTextAttribute(NumberFormatTextAttribute.PositiveSuffix),
            PlusString = decimalFormat.GetSymbol(NumberFormatSymbol.PlusSignSymbol),
            MinusString = decimalFormat.GetSymbol(NumberFormatSymbol.MinusSignSymbol),
            GroupingSeparatorChar = decimalFormat
                .GetSymbol(NumberFormatSymbol.GroupingSeparatorSymbol)
                .FirstOrDefault(','),
            DecimalSeparatorChar = decimalFormat
                .GetSymbol(NumberFormatSymbol.DecimalSeparatorSymbol)
                .FirstOrDefault('.'),
            PrimaryGroupingSize = (byte)decimalFormat.GroupingSize,
            SecondaryGroupingSize = (byte)decimalFormat.SecondaryGroupingSize,
            Digits =
            [
                decimalFormat.GetSymbol(NumberFormatSymbol.ZeroDigitSymbol).FirstOrDefault('0'),
                decimalFormat.GetSymbol(NumberFormatSymbol.OneDigitSymbol).FirstOrDefault('1'),
                decimalFormat.GetSymbol(NumberFormatSymbol.TwoDigitSymbol).FirstOrDefault('2'),
                decimalFormat.GetSymbol(NumberFormatSymbol.ThreeDigitSymbol).FirstOrDefault('3'),
                decimalFormat.GetSymbol(NumberFormatSymbol.FourDigitSymbol).FirstOrDefault('4'),
                decimalFormat.GetSymbol(NumberFormatSymbol.FiveDigitSymbol).FirstOrDefault('5'),
                decimalFormat.GetSymbol(NumberFormatSymbol.SixDigitSymbol).FirstOrDefault('6'),
                decimalFormat.GetSymbol(NumberFormatSymbol.SevenDigitSymbol).FirstOrDefault('7'),
                decimalFormat.GetSymbol(NumberFormatSymbol.EightDigitSymbol).FirstOrDefault('8'),
                decimalFormat.GetSymbol(NumberFormatSymbol.NineDigitSymbol).FirstOrDefault('9'),
            ],
        };
    }

    private static readonly Lock InvariantGregorianCalendarLock = new();
    private static readonly Calendar? InvariantGregorianCalendar = Calendar.Create(CalendarType.Gregorian);

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
        InvariantGregorianCalendar.Set(CalendarDateFields.Year, utcTime.Year);
        InvariantGregorianCalendar.Set(CalendarDateFields.Month, utcTime.Month - 1);
        InvariantGregorianCalendar.Set(CalendarDateFields.Date, utcTime.Day);
        InvariantGregorianCalendar.Set(CalendarDateFields.Hour, utcTime.Hour);
        InvariantGregorianCalendar.Set(CalendarDateFields.Minute, utcTime.Minute);
        InvariantGregorianCalendar.Set(CalendarDateFields.Second, utcTime.Second);
        InvariantGregorianCalendar.Set(CalendarDateFields.Millisecond, utcTime.Millisecond);
        return InvariantGregorianCalendar.Time;
    }

    private DecimalFormat DateTimeDecimalFormat
    {
        get
        {
            if (field is not null)
                return field;

            var formatter =
                DecimalFormat.CreateInstance(_locale.Id)
                ?? DecimalFormat.CreateInstance(InvariantLocale.Id)
                ?? throw new InvalidOperationException("Invariant culture decimal formatter is null.");

            var decimalSymbols = DecimalNumberFormattingRules;
            formatter.SetSymbol(NumberFormatSymbol.ZeroDigitSymbol, [decimalSymbols.Digits[0]]);
            formatter.SetSymbol(NumberFormatSymbol.OneDigitSymbol, [decimalSymbols.Digits[1]]);
            formatter.SetSymbol(NumberFormatSymbol.TwoDigitSymbol, [decimalSymbols.Digits[2]]);
            formatter.SetSymbol(NumberFormatSymbol.ThreeDigitSymbol, [decimalSymbols.Digits[3]]);
            formatter.SetSymbol(NumberFormatSymbol.FourDigitSymbol, [decimalSymbols.Digits[4]]);
            formatter.SetSymbol(NumberFormatSymbol.FiveDigitSymbol, [decimalSymbols.Digits[5]]);
            formatter.SetSymbol(NumberFormatSymbol.SixDigitSymbol, [decimalSymbols.Digits[6]]);
            formatter.SetSymbol(NumberFormatSymbol.SevenDigitSymbol, [decimalSymbols.Digits[7]]);
            formatter.SetSymbol(NumberFormatSymbol.EightDigitSymbol, [decimalSymbols.Digits[8]]);
            formatter.SetSymbol(NumberFormatSymbol.NineDigitSymbol, [decimalSymbols.Digits[9]]);

            formatter.IsGroupingUsed = false;

            field = formatter;
            return field;
        }
    }

    internal DateFormat GetDateFormatter(DateTimeFormatStyle dateStyle, string? timeZone)
    {
        _dateFormat ??= CreateDateFormat(_locale.Id, DateTimeDecimalFormat);
        return GetDateTimeFormatter(_dateFormat, DateFormatStyle.Ignore, dateStyle.ToIcuEnum(), timeZone);
    }

    internal DateFormat GetTimeFormatter(DateTimeFormatStyle timeStyle, string? timeZone)
    {
        _timeFormat ??= CreateTimeFormat(_locale.Id, DateTimeDecimalFormat);
        return GetDateTimeFormatter(_timeFormat, timeStyle.ToIcuEnum(), DateFormatStyle.Ignore, timeZone);
    }

    internal DateFormat GetDateTimeFormatter(
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string? timeZone
    )
    {
        _dateTimeFormat ??= CreateDateTimeFormat(_locale.Id, DateTimeDecimalFormat);
        return GetDateTimeFormatter(_dateTimeFormat, timeStyle.ToIcuEnum(), dateStyle.ToIcuEnum(), timeZone);
    }

    internal DateFormat GetDateTimeFormatter(ReadOnlySpan<char> pattern, string? timeZone)
    {
        var dateFormat =
            DateFormat.Create(pattern, _locale.Id, timeZone)
            ?? (
                DateFormat.Create(pattern, InvariantLocale.Id, timeZone)
                ?? throw new InvalidOperationException("Invariant culture date formatter is null.")
            );
        dateFormat.NumberFormat = DateTimeDecimalFormat;
        return dateFormat;
    }

    private DateFormat GetDateTimeFormatter(
        DateFormat defaultFormatter,
        DateFormatStyle timeStyle,
        DateFormatStyle dateStyle,
        string? timeZone
    )
    {
        var isDefaultTimeZone = string.IsNullOrWhiteSpace(timeZone);
        if (!isDefaultTimeZone)
        {
            var canonicalInputTimeZoneId = Calendar.GetCanonicalTimeZoneId(timeZone);
            var defaultTimeZoneId = defaultFormatter.TimeZoneId;
            var canonicalDefaultTimeZoneId = Calendar.GetCanonicalTimeZoneId(defaultTimeZoneId);

            isDefaultTimeZone = canonicalInputTimeZoneId == canonicalDefaultTimeZoneId;
            if (!isDefaultTimeZone)
            {
                timeZone = defaultTimeZoneId;
            }
        }

        var isDefault = dateStyle == DateFormatStyle.Default && isDefaultTimeZone;

        if (isDefault)
            return defaultFormatter;

        return DateFormat.Create(timeStyle, dateStyle, _locale.Id, timeZone)
            ?? DateFormat.Create(timeStyle, dateStyle, InvariantLocale.Id, timeZone)
            ?? throw new InvalidOperationException("Invariant culture date formatter is null.");
    }

    private static DateFormat CreateDateFormat(CultureId locale, DecimalFormat numberFormat)
    {
        var dateFormat =
            DateFormat.Create(DateFormatStyle.Ignore, DateFormatStyle.Default, locale, Calendar.DefaultTimeZone)
            ?? (
                DateFormat.Create(
                    DateFormatStyle.Default,
                    DateFormatStyle.Default,
                    InvariantLocale.Id,
                    Calendar.DefaultTimeZone
                ) ?? throw new InvalidOperationException("Invariant culture date formatter is null.")
            );
        dateFormat.NumberFormat = numberFormat;
        return dateFormat;
    }

    private static DateFormat CreateTimeFormat(CultureId locale, DecimalFormat numberFormat)
    {
        var dateFormat =
            DateFormat.Create(DateFormatStyle.Default, DateFormatStyle.Ignore, locale, Calendar.DefaultTimeZone)
            ?? (
                DateFormat.Create(
                    DateFormatStyle.Default,
                    DateFormatStyle.Default,
                    InvariantLocale.Id,
                    Calendar.DefaultTimeZone
                ) ?? throw new InvalidOperationException("Invariant culture date formatter is null.")
            );
        dateFormat.NumberFormat = numberFormat;
        return dateFormat;
    }

    private static DateFormat CreateDateTimeFormat(CultureId locale, DecimalFormat numberFormat)
    {
        var dateFormat =
            DateFormat.Create(DateFormatStyle.Default, DateFormatStyle.Default, locale, Calendar.DefaultTimeZone)
            ?? (
                DateFormat.Create(
                    DateFormatStyle.Default,
                    DateFormatStyle.Default,
                    InvariantLocale.Id,
                    Calendar.DefaultTimeZone
                ) ?? throw new InvalidOperationException("Invariant culture date formatter is null.")
            );
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
        var formTag = rules.Select(double.CreateTruncating(value));
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

    private Collator CreateCollator(Locale locale)
    {
        return Collator.Create(locale.Id)
            ?? Collator.Create(InvariantLocale.Id)
            ?? throw new InvalidOperationException("Invariant culture collator is null.");
    }

    private delegate int NativeGetInvariantString(
        CultureId locale,
        Span<char> buffer,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    private delegate int NativeGetTranslatedString(
        CultureId locale,
        CultureId displayLocale,
        Span<char> buffer,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    private static string GetNativeString(CultureId locale, Span<char> buffer, NativeGetInvariantString func)
    {
        var length = func(locale, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    private static string GetNativeString(CultureId locale, Span<char> buffer, NativeGetTranslatedString func)
    {
        var length = func(locale, locale, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    private static string GetNativeString(
        CultureId locale,
        CultureId displayLocale,
        Span<char> buffer,
        NativeGetTranslatedString func
    )
    {
        var length = func(locale, displayLocale, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    internal const string UnicodeLibName = "icuuc";
    internal const string I18NLibName = "icuin";

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getDefault", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string NativeGetDefault();

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getName")]
    private static partial int NativeGetName(
        CultureId targetLocal,
        CultureId displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getDisplayName")]
    private static partial int NativeGetDisplayName(
        CultureId targetLocal,
        CultureId displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getLanguage")]
    private static partial int NativeGetLanguage(
        CultureId targetLocal,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getScript")]
    private static partial int NativeGetScript(
        CultureId targetLocal,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getDisplayScript")]
    private static partial int NativeGetDisplayScript(
        CultureId targetLocal,
        CultureId displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getCountry")]
    private static partial int NativeGetCountry(
        CultureId targetLocal,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getDisplayCountry")]
    private static partial int NativeGetDisplayCountry(
        CultureId targetLocal,
        CultureId displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getVariant")]
    private static partial int NativeGetVariant(
        CultureId targetLocal,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getDisplayVariant")]
    private static partial int NativeGetDisplayVariant(
        CultureId targetLocal,
        CultureId displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getISO3Language", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string NativeGetISO3Language(CultureId targetLocal);

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getDisplayLanguage")]
    private static partial int NativeGetDisplayLanguage(
        CultureId targetLocal,
        CultureId displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_isRightToLeft")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsRightToLeft(CultureId targetLocal);

    [LibraryImport(UnicodeLibName, EntryPoint = "uloc_getLCID")]
    private static partial int NativeGetLCID(CultureId targetLocal);
}
