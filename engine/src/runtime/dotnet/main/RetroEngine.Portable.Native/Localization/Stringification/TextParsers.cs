// // @file Parsers.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Parsers;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization.Stringification;

using CustomDateFormatParser = TextParser<(
    DateTimeFormatStyle DateStyle,
    DateTimeFormatStyle TimeStyle,
    string? Format
)>;

public readonly record struct NumberParseResult(
    FormatNumericArg Value,
    NumberFormattingOptions? Options,
    Culture? TargetCulture
);

public readonly record struct DateTimeParseResult(
    DateTimeOffset Value,
    DateTimeFormatStyle DateStyle,
    DateTimeFormatStyle TimeStyle,
    string? CustomPattern,
    string TimeZone,
    Culture? TargetCulture
);

public static class TextParsers
{
    public static TextParser<TextSpan> Marker(string marker) => Span.EqualTo(marker);

    public static TextParser<TextSpan> InsensitiveMarker(string marker) => Span.EqualToIgnoreCase(marker);

    public static readonly TextParser<Unit> Whitespace = Character.WhiteSpace.IgnoreMany();

    public static TextParser<char> WhitespaceAndCharacter(char character)
    {
        return Whitespace.IgnoreThen(Character.EqualTo(character));
    }

    public static readonly TextParser<char> WhitespaceAndOpenParen = WhitespaceAndCharacter('(');

    public static readonly TextParser<char> WhitespaceAndComma = WhitespaceAndCharacter(',');

    public static readonly TextParser<char> WhitespaceAndCloseParen = WhitespaceAndCharacter(')');

    private static readonly TextParser<FormatNumericArg> Integer = Numerics
        .Integer.Select(c => long.Parse(c.AsReadOnlySpan()))
        .Select(FormatNumericArg.Signed);

    private static readonly TextParser<FormatNumericArg> Unsigned = Parse
        .Sequence(Numerics.Integer, Character.EqualTo('u'))
        .Select(c => long.Parse(c.Item1.AsReadOnlySpan()))
        .Select(FormatNumericArg.Signed);

    private static readonly TextParser<TextSpan> Decimal = Span.MatchedBy(
        Parse.Sequence(Numerics.Integer, Character.EqualTo('.').IgnoreThen(Numerics.Natural).OptionalOrDefault())
    );

    private static readonly TextParser<FormatNumericArg> Float = Parse
        .Sequence(Decimal, Character.EqualTo('f'))
        .Select(c => float.Parse(c.Item1.AsReadOnlySpan()))
        .Select(FormatNumericArg.Float);

    private static readonly TextParser<FormatNumericArg> Double = Decimal
        .Select(c => double.Parse(c.AsReadOnlySpan()))
        .Select(FormatNumericArg.Double);

    public static readonly TextParser<FormatNumericArg> Number = Float.Or(Double).Or(Unsigned).Or(Integer);

    public static readonly TextParser<TextSpan> AlphaNumeric = Span.MatchedBy(
        Character.LetterOrDigit.Or(Character.EqualTo('_')).IgnoreMany()
    );

    public static readonly TextParser<string> QuotedString = Marker(Markers.Text)
        .IgnoreThen(WhitespaceAndOpenParen)
        .IgnoreThen(StringLiteral.UnrealStyle)
        .FollowedBy(WhitespaceAndCloseParen)
        .Try()
        .Or(StringLiteral.UnrealStyle);

    public static TextParser<T> ScopedEnum<T>(string scopeName)
        where T : struct, Enum
    {
        return Marker(scopeName)
            .IgnoreThen(AlphaNumeric)
            .Apply(value =>
                Enum.TryParse<T>(value.AsReadOnlySpan(), out var result)
                    ? Result.Value(result, value, TextSpan.Empty)
                    : Result.Empty<T>(value, [typeof(T).Name])
            );
    }

    public static TextParser<T> NumberFormattingOption<T>(string optionFunctionName, TextParser<T> readOption)
    {
        return Marker(optionFunctionName)
            .IgnoreThen(WhitespaceAndOpenParen)
            .IgnoreThen(Whitespace)
            .IgnoreThen(readOption)
            .FollowedBy(WhitespaceAndCloseParen);
    }

    private static readonly TextParser<bool> BoolOption = Span.EqualToIgnoreCase("true")
        .Value(true)
        .Try()
        .Or(Span.EqualToIgnoreCase("false").Value(false));

    private static readonly TextParser<int> NumericOption = Numerics.IntegerInt32;

    private static readonly TextParser<RoundingMode> RoundingModeOption = ScopedEnum<RoundingMode>("ERoundingMode::");

    private static TextParser<T> CustomOption<T>(string name, TextParser<T> parser)
    {
        return Character.EqualTo('.').Optional().IgnoreThen(NumberFormattingOption($"Set{name}", parser));
    }

    private static readonly TextParser<bool> AlwaysSign = CustomOption(
        nameof(NumberFormattingOptions.AlwaysSign),
        BoolOption
    );

    private static readonly TextParser<bool> UseGrouping = CustomOption(
        nameof(NumberFormattingOptions.UseGrouping),
        BoolOption
    );

    private static readonly TextParser<RoundingMode> RoundingMode = CustomOption(
        nameof(NumberFormattingOptions.RoundingMode),
        RoundingModeOption
    );

    private static readonly TextParser<int> MinimumIntegralDigits = CustomOption(
        nameof(NumberFormattingOptions.MinimumIntegralDigits),
        NumericOption
    );

    private static readonly TextParser<int> MaximumIntegralDigits = CustomOption(
        nameof(NumberFormattingOptions.MaximumIntegralDigits),
        NumericOption
    );

    private static readonly TextParser<int> MinimumFractionalDigits = CustomOption(
        nameof(NumberFormattingOptions.MinimumFractionalDigits),
        NumericOption
    );

    private static readonly TextParser<int> MaximumFractionalDigits = CustomOption(
        nameof(NumberFormattingOptions.MaximumFractionalDigits),
        NumericOption
    );

    public static readonly TextParser<NumberFormattingOptions> NumberFormatOptions = input =>
    {
        var builder = new NumberFormattingOptionsBuilder();

        bool didReadOption;
        var remainder = input;
        do
        {
            didReadOption = false;

            var alwaysSign = AlwaysSign(remainder);
            if (alwaysSign.HasValue)
            {
                builder.AlwaysSign = alwaysSign.Value;
                remainder = alwaysSign.Remainder;
                didReadOption = true;
            }

            var useGrouping = UseGrouping(remainder);
            if (useGrouping.HasValue)
            {
                builder.UseGrouping = useGrouping.Value;
                remainder = useGrouping.Remainder;
                didReadOption = true;
            }

            var roundingMode = RoundingMode(remainder);
            if (roundingMode.HasValue)
            {
                builder.RoundingMode = roundingMode.Value;
                remainder = roundingMode.Remainder;
                didReadOption = true;
            }

            var minimumIntegralDigits = MinimumIntegralDigits(remainder);
            if (minimumIntegralDigits.HasValue)
            {
                builder.MinimumIntegralDigits = minimumIntegralDigits.Value;
                remainder = minimumIntegralDigits.Remainder;
                didReadOption = true;
            }

            var maximumIntegralDigits = MaximumIntegralDigits(remainder);
            if (maximumIntegralDigits.HasValue)
            {
                builder.MaximumIntegralDigits = maximumIntegralDigits.Value;
                remainder = maximumIntegralDigits.Remainder;
                didReadOption = true;
            }

            var minimumFractionalDigits = MinimumFractionalDigits(remainder);
            if (minimumFractionalDigits.HasValue)
            {
                builder.MinimumFractionalDigits = minimumFractionalDigits.Value;
                remainder = minimumFractionalDigits.Remainder;
                didReadOption = true;
            }

            var maximumFractionalDigits = MaximumFractionalDigits(remainder);
            if (!maximumFractionalDigits.HasValue)
                continue;

            builder.MaximumFractionalDigits = maximumFractionalDigits.Value;
            remainder = maximumFractionalDigits.Remainder;
            didReadOption = true;
        } while (didReadOption);

        return Result.Value(builder.Build(), input, remainder);
    };

    public static readonly TextParser<Culture?> CultureFromName = QuotedString.Select(c =>
        CultureManager.Instance.GetCulture(c)
    );

    private static readonly TextParser<NumberParseResult> CustomFormatSuffix = Parse
        .Sequence(
            Marker(Markers.CustomSuffix).IgnoreThen(WhitespaceAndOpenParen).IgnoreThen(Number),
            WhitespaceAndComma.IgnoreThen(Whitespace).IgnoreThen(NumberFormatOptions),
            WhitespaceAndComma.IgnoreThen(Whitespace).IgnoreThen(CultureFromName).FollowedBy(WhitespaceAndCloseParen)
        )
        .Select(r => new NumberParseResult(r.Item1, r.Item2, r.Item3));

    private static readonly TextParser<NumberParseResult> StandardFormatSuffix = Parse
        .Sequence(
            Marker(Markers.GroupedSuffix)
                .Value((NumberFormattingOptions?)NumberFormattingOptions.DefaultWithGrouping)
                .Or(
                    Marker(Markers.UngroupedSuffix)
                        .Value((NumberFormattingOptions?)NumberFormattingOptions.DefaultWithoutGrouping)
                )
                .OptionalOrDefault(),
            WhitespaceAndOpenParen.IgnoreThen(Number),
            WhitespaceAndComma.IgnoreThen(Whitespace).IgnoreThen(CultureFromName).FollowedBy(WhitespaceAndCloseParen)
        )
        .Select(r => new NumberParseResult(r.Item2, r.Item1, r.Item3));

    private static readonly TextParser<NumberParseResult> FormatSuffix = CustomFormatSuffix
        .Try()
        .Or(StandardFormatSuffix);

    public static TextParser<NumberParseResult> NumberOrPercent(string marker)
    {
        return Span.EqualTo(marker).IgnoreThen(FormatSuffix);
    }

    private static readonly TextParser<DateTimeFormatStyle> DateTimeStyle = ScopedEnum<DateTimeFormatStyle>(
        "EDateTimeStyle::"
    );

    private static readonly TextParser<DateTimeFormatStyle> DateTimeArg = WhitespaceAndComma
        .IgnoreThen(Whitespace)
        .IgnoreThen(DateTimeStyle);

    private static readonly TextParser<(bool IsCustom, string? TimeZone)> DateTimeSuffix = Parse.Sequence(
        Marker(Markers.CustomSuffix).Value(true).OptionalOrDefault(),
        Marker(Markers.LocalSuffix)
            .Value((string?)Text.InvariantTimeZone)
            .Or(Marker(Markers.UtcSuffix).Value((string?)null))
    );

    private static readonly TextParser<string> QuotedStringArg = WhitespaceAndComma
        .IgnoreThen(Whitespace)
        .IgnoreThen(QuotedString);

    private static readonly CustomDateFormatParser CustomDateFormat = QuotedStringArg.Select(s =>
        (DateTimeFormatStyle.Custom, DateTimeFormatStyle.Custom, (string?)s)
    );

    private static readonly CustomDateFormatParser DateOnlyFormat = DateTimeArg.Select(s =>
        (s, DateTimeFormatStyle.Default, (string?)null)
    );

    private static readonly CustomDateFormatParser TimeOnlyFormat = DateTimeArg.Select(s =>
        (DateTimeFormatStyle.Default, s, (string?)null)
    );

    private static readonly CustomDateFormatParser FullDateTimeFormat = Parse
        .Sequence(DateTimeArg, DateTimeArg)
        .Select((t) => (t.Item1, t.Item1, (string?)null));

    private static readonly TextParser<(bool IsCustom, string? TimeZone, DateTimeOffset DateTime)> DateTimeInfo = Parse
        .Sequence(
            DateTimeSuffix,
            WhitespaceAndOpenParen
                .IgnoreThen(Whitespace)
                .IgnoreThen(Numerics.IntegerInt64)
                .Select(DateTimeOffset.FromUnixTimeMilliseconds)
        )
        .Select(t => (t.Item1.IsCustom, t.Item1.TimeZone, DateTime: t.Item2));

    public static TextParser<DateTimeParseResult> DateTime(string marker, bool includeDate, bool includeTime)
    {
        if (!includeDate && !includeTime)
            throw new ArgumentException("At least one of includeDate or includeTime must be true");

        var parseBasicData = Marker(marker)
            .IgnoreThen(DateTimeInfo)
            .SelectMany(
                t =>
                {
                    if (t.IsCustom)
                    {
                        return CustomDateFormat;
                    }

                    if (includeDate && includeTime)
                    {
                        return FullDateTimeFormat;
                    }

                    return includeDate ? DateOnlyFormat : TimeOnlyFormat;
                },
                (prefix, format) =>
                    (
                        prefix.IsCustom,
                        prefix.TimeZone,
                        prefix.DateTime,
                        format.DateStyle,
                        format.TimeStyle,
                        format.Format
                    )
            );

        return input =>
        {
            var initialInfo = parseBasicData(input);
            if (!initialInfo.HasValue)
                return Result.CastEmpty<
                    (bool, string?, DateTimeOffset, DateTimeFormatStyle, DateTimeFormatStyle, string?),
                    DateTimeParseResult
                >(initialInfo);

            var (isCustom, timeZone, dateTime, dateStyle, timeStyle, format) = initialInfo.Value;
            var remainder = initialInfo.Remainder;

            if (timeZone is null)
            {
                var timeZoneResult = QuotedStringArg(remainder);
                if (!timeZoneResult.HasValue)
                    return Result.CastEmpty<string, DateTimeParseResult>(timeZoneResult);

                timeZone = timeZoneResult.Value;
                remainder = timeZoneResult.Remainder;
            }

            var cultureResult = QuotedStringArg(remainder);
            if (!cultureResult.HasValue)
                return Result.CastEmpty<string, DateTimeParseResult>(cultureResult);

            var closeParentResult = WhitespaceAndCloseParen(remainder);
            if (!closeParentResult.HasValue)
                return Result.CastEmpty<char, DateTimeParseResult>(closeParentResult);

            var culture = CultureManager.Instance.GetCulture(cultureResult.Value);
            return Result.Value(
                new DateTimeParseResult(dateTime, dateStyle, timeStyle, format, timeZone, culture),
                initialInfo.Location,
                remainder
            );
        };
    }

    public static TextParser<Text> ExportedText => throw new NotImplementedException();

    public static TextParser<Text> QuotedText => throw new NotImplementedException();
}
