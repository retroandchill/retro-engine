// // @file TextStringReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
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
    private delegate void CustomOptionParser<in T>(scoped ref NumberFormattingOptionsBuilder builder, T value);

    private delegate ParseResult<bool> StatefulParser<TState>(scoped ref TState state, TextSegment input);

    extension(TextSegment input)
    {
        public ParseResult<FormatNumericArg> ParseNumber()
        {
            var literal = input.ParseDecimalLiteral();
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, FormatNumericArg>(literal);

            var suffix = literal.Remainder.ParseCharIn('f', 'u');
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
        }

        private ParseResult<bool> ParseNumberFormattingOption<T>(
            string marker,
            Func<TextSegment, ParseResult<T>> readOption,
            scoped ref NumberFormattingOptionsBuilder builder,
            CustomOptionParser<T> applyValue
        )
        {
            var identifier = input.ParseSymbol(marker);
            if (!identifier.HasValue)
                return ParseResult.Success(false, input, input);

            var openParen = identifier.Remainder.ParseWhitespaceAndChar('(');
            if (!openParen.HasValue)
                return ParseResult.CastEmpty<TextSegment, bool>(openParen);

            var value = readOption(openParen.Remainder.ParseOptionalWhitespace().Remainder);
            if (!value.HasValue)
                return ParseResult.CastEmpty<T, bool>(value);

            applyValue(ref builder, value.Value);

            var closeParent = value.Remainder.ParseWhitespaceAndChar(')');
            if (!closeParent.HasValue)
                return ParseResult.CastEmpty<TextSegment, bool>(closeParent);

            var remainder = closeParent.Remainder.ParseOptionalWhitespace().Remainder;
            return ParseResult.Success(true, input, remainder);
        }

        private ParseResult<Unit> ParseNumberFormattingOptions<TState>(
            scoped ref TState state,
            params ReadOnlySpan<StatefulParser<TState>> parsers
        )
        {
            if (parsers.Length == 0)
                throw new ArgumentException("parsers must not be empty", nameof(parsers));

            var remainder = input;
            var didReadOption = true;
            var firstElement = true;
            while (didReadOption)
            {
                didReadOption = false;
                foreach (var parser in parsers)
                {
                    if (!firstElement)
                    {
                        var delimiterResult = remainder.ParseChar('.');
                        if (delimiterResult.HasValue)
                            remainder = delimiterResult.Remainder;
                        else
                            break;
                    }

                    var result = parser(ref state, remainder);
                    if (!result.HasValue)
                    {
                        return ParseResult.CastEmpty<bool, Unit>(result);
                    }

                    if (!result.Value)
                        continue;
                    didReadOption = true;
                    firstElement = false;
                }
            }

            return ParseResult.Success(Unit.Value, input, remainder);
        }

        public ParseResult<NumberFormattingOptions> ParseNumberFormattingOptions()
        {
            var builder = new NumberFormattingOptionsBuilder();
            var readOptions = input.ParseNumberFormattingOptions(
                ref builder,
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.AlwaysSign)}",
                        c => c.ParseBool(),
                        ref b,
                        (scoped ref o, c) => o.AlwaysSign = c
                    ),
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.UseGrouping)}",
                        c => c.ParseBool(),
                        ref b,
                        (scoped ref o, c) => o.UseGrouping = c
                    ),
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.RoundingMode)}",
                        c => c.ParseEnum<RoundingMode>("ERoundingMode::"),
                        ref b,
                        (scoped ref o, c) => o.RoundingMode = c
                    ),
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.MinimumIntegralDigits)}",
                        c => c.ParseSignedInteger<int>(),
                        ref b,
                        (scoped ref o, c) => o.MinimumIntegralDigits = c
                    ),
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.MaximumIntegralDigits)}",
                        c => c.ParseSignedInteger<int>(),
                        ref b,
                        (scoped ref o, c) => o.MaximumIntegralDigits = c
                    ),
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.MinimumFractionalDigits)}",
                        c => c.ParseSignedInteger<int>(),
                        ref b,
                        (scoped ref o, c) => o.MinimumFractionalDigits = c
                    ),
                (scoped ref b, i) =>
                    i.ParseNumberFormattingOption(
                        $"Set{nameof(NumberFormattingOptions.MaximumFractionalDigits)}",
                        c => c.ParseSignedInteger<int>(),
                        ref b,
                        (scoped ref o, c) => o.MaximumFractionalDigits = c
                    )
            );

            return readOptions.HasValue
                ? ParseResult.Success(builder.Build(), input, readOptions.Remainder)
                : ParseResult.CastEmpty<Unit, NumberFormattingOptions>(readOptions);
        }

        public ParseResult<NumberParseResult> ParseNumberOrPercent(string marker)
        {
            var identifier = input.ParseSymbol(marker);
            if (!identifier.HasValue)
                return ParseResult.CastEmpty<TextSegment, NumberParseResult>(identifier);

            TextSegment remainder;
            NumberFormattingOptions? options = null;
            var customSuffix = identifier.Remainder.ParseSymbol(Markers.CustomSuffix);
            if (customSuffix.HasValue)
            {
                remainder = customSuffix.Remainder;
            }
            else if (identifier.Remainder.ParseSymbol(Markers.GroupedSuffix) is { HasValue: true } groupedSuffix)
            {
                remainder = groupedSuffix.Remainder;
                options = NumberFormattingOptions.DefaultWithGrouping;
            }
            else if (identifier.Remainder.ParseSymbol(Markers.UngroupedSuffix) is { HasValue: true } ungroupedSuffix)
            {
                remainder = ungroupedSuffix.Remainder;
                options = NumberFormattingOptions.DefaultWithoutGrouping;
            }
            else
            {
                remainder = identifier.Remainder;
            }

            var number = remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar('('),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseNumber(),
                (_, _, i) => i
            );
            if (!number.HasValue)
                return ParseResult.CastEmpty<FormatNumericArg, NumberParseResult>(number);
            remainder = number.Remainder;

            if (customSuffix.HasValue)
            {
                var optionsResult = remainder.ParseSequence(
                    i => i.ParseWhitespaceAndChar(','),
                    i => i.ParseOptionalWhitespace(),
                    i => i.ParseNumberFormattingOptions(),
                    (_, _, i) => i
                );

                if (!optionsResult.HasValue)
                    return ParseResult.CastEmpty<NumberFormattingOptions, NumberParseResult>(optionsResult);

                options = optionsResult.Value;
                remainder = optionsResult.Remainder;
            }

            var targetCulture = remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar(','),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                (_, _, i) => CultureManager.Instance.GetCulture(i)
            );
            if (!targetCulture.HasValue)
                return ParseResult.CastEmpty<Culture?, NumberParseResult>(targetCulture);

            var closeParen = targetCulture.Remainder.ParseWhitespaceAndChar(')');
            if (!closeParen.HasValue)
                return ParseResult.CastEmpty<TextSegment, NumberParseResult>(closeParen);

            return ParseResult.Success(
                new NumberParseResult(number.Value, options, targetCulture.Value),
                input,
                closeParen.Remainder
            );
        }

        public ParseResult<DateTimeParseResult> ParseDateTime(string marker, bool includeDate, bool includeTime)
        {
            var identifier = input.ParseSymbol(marker);
            if (!identifier.HasValue)
                return ParseResult.CastEmpty<TextSegment, DateTimeParseResult>(identifier);

            var custom = identifier.Remainder.ParseSymbol(Markers.CustomSuffix);
            var remainder = custom.HasValue ? custom.Remainder : identifier.Remainder;

            string? timeZone;
            if (remainder.ParseSymbol(Markers.LocalSuffix) is { HasValue: true } localSuffix)
            {
                remainder = localSuffix.Remainder;
                timeZone = Text.InvariantTimeZone;
            }
            else if (remainder.ParseSymbol(Markers.UtcSuffix) is { HasValue: true } utcSuffix)
            {
                remainder = utcSuffix.Remainder;
                timeZone = null;
            }
            else
            {
                return ParseResult.Empty<DateTimeParseResult>(remainder);
            }

            var dateTime = remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar('('),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseSignedInteger<long>(),
                (_, _, i) => DateTimeOffset.FromUnixTimeMilliseconds(i)
            );
            if (!dateTime.HasValue)
                return ParseResult.CastEmpty<DateTimeOffset, DateTimeParseResult>(dateTime);
            remainder = dateTime.Remainder;

            var dateStyle = DateTimeFormatStyle.Default;
            var timeStyle = DateTimeFormatStyle.Default;
            string? format = null;
            if (custom.HasValue)
            {
                if (includeDate)
                {
                    dateStyle = DateTimeFormatStyle.Custom;
                }

                if (includeTime)
                {
                    timeStyle = DateTimeFormatStyle.Custom;
                }

                var formatPattern = remainder.ParseSequence(
                    i => i.ParseWhitespaceAndChar(','),
                    i => i.ParseOptionalWhitespace(),
                    i => i.ParseQuotedString(),
                    (_, _, i) => i
                );
                if (!formatPattern.HasValue)
                    return ParseResult.Empty<DateTimeParseResult>(remainder);

                format = formatPattern.Value;
                remainder = formatPattern.Remainder;
            }
            else
            {
                const string dateStyleMarker = "EDateTimeStyle::";

                if (includeDate)
                {
                    var styleResult = remainder.ParseSequence(
                        i => i.ParseWhitespaceAndChar(','),
                        i => i.ParseOptionalWhitespace(),
                        i => i.ParseEnum<DateTimeFormatStyle>(dateStyleMarker),
                        (_, _, i) => i
                    );
                    if (!styleResult.HasValue)
                        return ParseResult.Empty<DateTimeParseResult>(remainder);

                    dateStyle = styleResult.Value;
                    remainder = styleResult.Remainder;
                }

                if (includeTime)
                {
                    var styleResult = remainder.ParseSequence(
                        i => i.ParseWhitespaceAndChar(','),
                        i => i.ParseOptionalWhitespace(),
                        i => i.ParseEnum<DateTimeFormatStyle>(dateStyleMarker),
                        (_, _, i) => i
                    );
                    if (!styleResult.HasValue)
                        return ParseResult.Empty<DateTimeParseResult>(remainder);

                    timeStyle = styleResult.Value;
                    remainder = styleResult.Remainder;
                }
            }

            if (timeZone is null)
            {
                var timeZoneResult = remainder.ParseSequence(
                    i => i.ParseWhitespaceAndChar(','),
                    i => i.ParseOptionalWhitespace(),
                    i => i.ParseQuotedString(),
                    (_, _, i) => i
                );
                if (!timeZoneResult.HasValue)
                    return ParseResult.CastEmpty<string, DateTimeParseResult>(timeZoneResult);

                timeZone = timeZoneResult.Value;
                remainder = timeZoneResult.Remainder;
            }

            var targetCulture = remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar(','),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                (_, _, i) => CultureManager.Instance.GetCulture(i)
            );
            if (!targetCulture.HasValue)
                return ParseResult.CastEmpty<Culture?, DateTimeParseResult>(targetCulture);

            var closeParen = targetCulture.Remainder.ParseWhitespaceAndChar(')');
            if (!closeParen.HasValue)
                return ParseResult.CastEmpty<TextSegment, DateTimeParseResult>(closeParen);

            return ParseResult.Success(
                new DateTimeParseResult(dateTime.Value, dateStyle, timeStyle, format, timeZone, targetCulture.Value),
                input,
                closeParen.Remainder
            );
        }
    }

    private static readonly TextParser<char> WhitespaceAndOpeningParen = Sequences
        .Whitespace.OrElseDefault()
        .IgnoreThen(Characters.EqualTo(')'));

    private static readonly TextParser<char> WhitespaceAndClosingParen = Sequences
        .Whitespace.OrElseDefault()
        .IgnoreThen(Characters.EqualTo(')'));

    private static readonly TextParser<char> WhitespaceThenComma = Sequences.Whitespace.IgnoreThen(
        Characters.EqualTo(',')
    );

    public static TextParser<T> Marked<T>(string marker, TextParser<T> parser)
        where T : allows ref struct
    {
        return Sequences
            .EqualTo(marker)
            .IgnoreThen(WhitespaceAndOpeningParen)
            .Then(Sequences.Whitespace.OrElseDefault().IgnoreThen(parser), (_, r) => r)
            .FollowedBy(WhitespaceAndClosingParen);
    }

    public static TextParser<TResult> CommaSeparatedList<T, TResult>(
        this TextParser<T> parser,
        int elements,
        Func<ReadOnlySpan<T>, TResult> create
    )
        where TResult : allows ref struct
    {
        return WhitespaceThenComma.IgnoreThen(
            parser.RepeatDelimitedBy(
                elements,
                WhitespaceThenComma,
                () => (Array: ArrayPool<T>.Shared.Rent(elements), Count: 0),
                (l, x) =>
                {
                    l.Array[l.Count++] = x;
                    return l;
                },
                l => create(l.Array.AsSpan(0, l.Count)),
                l => ArrayPool<T>.Shared.Return(l.Array, true)
            )
        );
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
        Marked(Markers.Text, StringLiterals.QuotedString).Or(StringLiterals.QuotedString);

    private static readonly TextParser<TextSegment> SetPrefix = Sequences.EqualTo("Set");

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
            if (_type != NumberFormattingOptionType.Integer)
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
        OptionsParsers.Many(
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
}
