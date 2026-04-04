// // @file TextStringReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Stringification;

internal readonly record struct NumberParseResult(
    FormatNumericArg Value,
    NumberFormattingOptions? Options,
    Culture? TargetCulture
);

internal readonly record struct DateTimeParseResult(
    DateTimeOffset Value,
    DateTimeFormatStyle DateStyle,
    DateTimeFormatStyle TimeStyle,
    string? CustomPattern,
    string TimeZone,
    Culture? TargetCulture
);

internal static class TextStringReader
{
    public static readonly TextParser<char> WhitespaceAndOpeningParen = Sequences
        .OptionalWhitespace.IgnoreThen(Characters.EqualTo('('))
        .FollowedBy(Sequences.OptionalWhitespace);

    public static readonly TextParser<char> WhitespaceAndClosingParen = Sequences.OptionalWhitespace.IgnoreThen(
        Characters.EqualTo(')')
    );

    public static readonly TextParser<char> CommaSeparator = Sequences
        .OptionalWhitespace.IgnoreThen(Characters.EqualTo(','))
        .FollowedBy(Sequences.OptionalWhitespace);

    public static TextParser<T> Marked<T>(string marker, TextParser<T> parser)
        where T : allows ref struct
    {
        return Sequences
            .EqualTo(marker)
            .IgnoreThen(WhitespaceAndOpeningParen)
            .Then(parser, (_, r) => r)
            .FollowedBy(WhitespaceAndClosingParen);
    }

    private static readonly TextParser<char> NumberSuffix = Characters.In('f', 'u');

    public static TextParser<FormatNumericArg> Number { get; } =
        input =>
        {
            var literal = Numerics.DecimalLiteral(input);
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, FormatNumericArg>(literal);

            var suffix = NumberSuffix(literal.Remainder);
            return suffix switch
            {
                { HasValue: true, Value: 'f' } => float.TryParse(literal.Value.Segment, out var f)
                    ? ParseResult.Success(FormatNumericArg.Float(f), input, suffix.Remainder)
                    : ParseResult.Empty<FormatNumericArg>(input, suffix.Remainder),
                { HasValue: true, Value: 'u' } when literal.Value.IsUnsigned => ulong.TryParse(
                    literal.Value.Segment,
                    out var u
                )
                    ? ParseResult.Success(FormatNumericArg.Unsigned(u), input, suffix.Remainder)
                    : ParseResult.Empty<FormatNumericArg>(input, suffix.Remainder),
                { HasValue: false } when literal.Value.IsInteger => long.TryParse(literal.Value.Segment, out var i)
                    ? ParseResult.Success(FormatNumericArg.Signed(i), input, suffix.Remainder)
                    : ParseResult.Empty<FormatNumericArg>(input, literal.Remainder),
                _ => double.TryParse(literal.Value.Segment, out var d)
                    ? ParseResult.Success(FormatNumericArg.Double(d), input, suffix.Remainder)
                    : ParseResult.Empty<FormatNumericArg>(input, literal.Remainder),
            };
        };

    public static TextParser<string> TextLiteral { get; } =
        Marked(Markers.Text, QuotedString.CStyle).Or(QuotedString.CStyle);

    private enum NumberFormattingOptionType : byte
    {
        Boolean,
        Integer,
        RoundingOption,
    }

    private readonly struct NumberFormattingOptionValue
    {
        private readonly NumberFormattingOptionType _type;
        private readonly int _value;

        public NumberFormattingOptionValue(bool value)
        {
            _type = NumberFormattingOptionType.Boolean;
            _value = value ? 1 : 0;
        }

        public NumberFormattingOptionValue(int value)
        {
            _type = NumberFormattingOptionType.Integer;
            _value = value;
        }

        public NumberFormattingOptionValue(RoundingMode value)
        {
            _type = NumberFormattingOptionType.RoundingOption;
            _value = (int)value;
        }

        public bool TryGetValue(out bool value)
        {
            if (_type != NumberFormattingOptionType.Boolean)
            {
                value = false;
                return false;
            }

            value = _value != 0;
            return true;
        }

        public bool TryGetValue(out int value)
        {
            if (_type != NumberFormattingOptionType.Integer)
            {
                value = 0;
                return false;
            }

            value = _value;
            return true;
        }

        public bool TryGetValue(out RoundingMode value)
        {
            if (_type != NumberFormattingOptionType.RoundingOption)
            {
                value = default;
                return false;
            }

            value = (RoundingMode)_value;
            return true;
        }

        public static implicit operator NumberFormattingOptionValue(bool value) => new(value);

        public static implicit operator NumberFormattingOptionValue(int value) => new(value);

        public static implicit operator NumberFormattingOptionValue(RoundingMode value) => new(value);
    }

    private static TextParser<(string Name, NumberFormattingOptionValue Value)> NumberFormattingOption(
        Expression<Func<NumberFormattingOptions, bool>> property,
        TextParser<bool> parser
    )
    {
        var propertyName = property.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            _ => throw new ArgumentException("Property must be a member expression", nameof(property)),
        };

        return Marked($"Set{propertyName}", parser).Select(x => (propertyName, new NumberFormattingOptionValue(x)));
    }

    private static TextParser<(string Name, NumberFormattingOptionValue Value)> NumberFormattingOption(
        Expression<Func<NumberFormattingOptions, int>> property,
        TextParser<int> parser
    )
    {
        var propertyName = property.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            _ => throw new ArgumentException("Property must be a member expression", nameof(property)),
        };

        return Marked($"Set{propertyName}", parser).Select(x => (propertyName, new NumberFormattingOptionValue(x)));
    }

    private static TextParser<(string Name, NumberFormattingOptionValue Value)> NumberFormattingOption(
        Expression<Func<NumberFormattingOptions, RoundingMode>> property,
        TextParser<RoundingMode> parser
    )
    {
        var propertyName = property.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            _ => throw new ArgumentException("Property must be a member expression", nameof(property)),
        };

        return Marked($"Set{propertyName}", parser).Select(x => (propertyName, new NumberFormattingOptionValue(x)));
    }

    private static TextParser<(string Name, NumberFormattingOptionValue Value)> OptionsParsers { get; } =
        NumberFormattingOption(x => x.AlwaysSign, Symbols.Boolean)
            .Or(
                NumberFormattingOption(x => x.UseGrouping, Symbols.Boolean),
                NumberFormattingOption(x => x.RoundingMode, Symbols.EnumLiteral<RoundingMode>("ERoundingMode::")),
                NumberFormattingOption(x => x.MinimumIntegralDigits, Numerics.Int),
                NumberFormattingOption(x => x.MaximumIntegralDigits, Numerics.Int),
                NumberFormattingOption(x => x.MinimumFractionalDigits, Numerics.Int),
                NumberFormattingOption(x => x.MaximumFractionalDigits, Numerics.Int)
            );

    public static TextParser<NumberFormattingOptions> NumberFormatting { get; } =
        OptionsParsers.ManyDelimitedBy(
            Characters.EqualTo('.'),
            () => new NumberFormattingOptionsBuilder(),
            (builder, option) =>
            {
                switch (option.Name)
                {
                    case nameof(NumberFormattingOptions.AlwaysSign):
                    {
                        builder.AlwaysSign = option.Value.TryGetValue(out bool v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                    case nameof(NumberFormattingOptions.UseGrouping):
                    {
                        builder.UseGrouping = option.Value.TryGetValue(out bool v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                    case nameof(NumberFormattingOptions.RoundingMode):
                    {
                        builder.RoundingMode = option.Value.TryGetValue(out RoundingMode v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                    case nameof(NumberFormattingOptions.MinimumIntegralDigits):
                    {
                        builder.MinimumIntegralDigits = option.Value.TryGetValue(out int v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                    case nameof(NumberFormattingOptions.MaximumIntegralDigits):
                    {
                        builder.MaximumIntegralDigits = option.Value.TryGetValue(out int v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                    case nameof(NumberFormattingOptions.MinimumFractionalDigits):
                    {
                        builder.MinimumFractionalDigits = option.Value.TryGetValue(out int v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                    case nameof(NumberFormattingOptions.MaximumFractionalDigits):
                    {
                        builder.MaximumFractionalDigits = option.Value.TryGetValue(out int v)
                            ? v
                            : throw new InvalidOperationException();
                        break;
                    }
                }

                return builder;
            },
            b => b.Build()
        );

    public static TextParser<Culture?> CultureByName { get; } =
        TextLiteral.Select(x => !string.IsNullOrEmpty(x) ? CultureManager.Instance.GetCulture(x) : null);

    private static readonly TextParser<NumberParseResult> NumberOrPercentCustom = Marked(
        Markers.CustomSuffix,
        Sequences
            .OptionalWhitespace.IgnoreThen(Number)
            .Then(CommaSeparator.IgnoreThen(NumberFormatting), (n, o) => (SourceValue: n, Options: o))
            .Then(
                CommaSeparator.IgnoreThen(CultureByName),
                (t, c) => new NumberParseResult(t.SourceValue, t.Options, c)
            )
    );

    private static readonly TextParser<NumberParseResult> NumberParseResultStandard = Sequences
        .EqualTo(Markers.GroupedSuffix)
        .Value((NumberFormattingOptions?)NumberFormattingOptions.DefaultWithGrouping)
        .Or(
            Sequences
                .EqualTo(Markers.UngroupedSuffix)
                .Value((NumberFormattingOptions?)NumberFormattingOptions.DefaultWithoutGrouping)
        )
        .OrElse(() => null)
        .Then(WhitespaceAndOpeningParen.IgnoreThen(Number), (o, n) => (SourceValue: n, Options: o))
        .Then(CommaSeparator.IgnoreThen(CultureByName), (t, c) => new NumberParseResult(t.SourceValue, t.Options, c))
        .FollowedBy(WhitespaceAndClosingParen);

    public static TextParser<NumberParseResult> NumberOrPercent(string marker)
    {
        return Sequences.EqualTo(marker).IgnoreThen(NumberOrPercentCustom.Or(NumberParseResultStandard));
    }

    private static readonly TextParser<DateTimeFormatStyle> DateStyle = Symbols.EnumLiteral<DateTimeFormatStyle>(
        "EDateTimeStyle::"
    );

    public static TextParser<DateTimeParseResult> DateTime(string marker, bool includeDate, bool includeTime)
    {
        var customStyleParser = CommaSeparator
            .IgnoreThen(TextLiteral)
            .Select(t =>
                (
                    DateStyle: DateTimeFormatStyle.Custom,
                    TimeStyle: DateTimeFormatStyle.Custom,
                    CustomPattern: (string?)t
                )
            );

        var regularStyleParser = includeDate switch
        {
            true when includeTime => CommaSeparator
                .IgnoreThen(DateStyle)
                .Then(
                    CommaSeparator.IgnoreThen(DateStyle),
                    (d, t) => (DateStyle: d, TimeStyle: t, CustomPattern: (string?)null)
                ),
            true when !includeTime => CommaSeparator
                .IgnoreThen(DateStyle)
                .Select(d => (DateStyle: d, TimeStyle: DateTimeFormatStyle.Default, CustomPattern: (string?)null)),
            false when includeTime => CommaSeparator
                .IgnoreThen(DateStyle)
                .Select(t => (DateStyle: DateTimeFormatStyle.Default, TimeStyle: t, CustomPattern: (string?)null)),
            _ => throw new ArgumentException("Invalid combination of includeDate and includeTime"),
        };

        var timeZoneParser = CommaSeparator.IgnoreThen(TextLiteral);
        var noTimeZoneParser = Parse.Return(string.Empty);

        return Sequences
            .EqualTo(marker)
            .IgnoreThen(Sequences.EqualTo(Markers.CustomSuffix).Value(true).OrElseDefault())
            .Then(
                Sequences
                    .EqualTo(Markers.LocalSuffix)
                    .Value(Text.InvariantTimeZone)
                    .Or(Sequences.EqualTo(Markers.UtcSuffix).Value("")),
                (c, t) => (IsCustom: c, TimeZone: t)
            )
            .Then(
                WhitespaceAndOpeningParen.IgnoreThen(Numerics.Long),
                (t, ts) => (t.IsCustom, t.TimeZone, Timestamp: DateTimeOffset.FromUnixTimeMilliseconds(ts))
            )
            .SelectMany(
                t => t.IsCustom ? customStyleParser : regularStyleParser,
                (t, c) => (t.IsCustom, t.TimeZone, t.Timestamp, c.DateStyle, c.TimeStyle, c.CustomPattern)
            )
            .SelectMany(
                t => string.IsNullOrEmpty(t.TimeZone) ? timeZoneParser : noTimeZoneParser,
                (t, tz) => string.IsNullOrEmpty(tz) ? t : t with { TimeZone = tz }
            )
            .Then(
                CommaSeparator.IgnoreThen(CultureByName),
                (t, c) => new DateTimeParseResult(t.Timestamp, t.DateStyle, t.TimeStyle, t.CustomPattern, t.TimeZone, c)
            );
    }

    public static TextParser<Text> QuotedText { get; } =
        input => TextStringifier.ImportFromString(input, requiresQuotes: true);
}
