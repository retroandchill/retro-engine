// // @file FormatArgumentValue.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Utils;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

public enum TextGender : byte
{
    Masculine,
    Feminine,
    Neuter,
}

[Union]
public readonly partial struct FormatArg
{
    [UnionCase]
    public static partial FormatArg Signed(long value);

    [UnionCase]
    public static partial FormatArg Unsigned(ulong value);

    [UnionCase]
    public static partial FormatArg Float(float value);

    [UnionCase]
    public static partial FormatArg Double(double value);

    [UnionCase]
    public static partial FormatArg Text(Text value);

    [UnionCase]
    public static partial FormatArg Gender(TextGender value);

    public bool IdenticalTo(FormatArg other, TextIdenticalModeFlags flags)
    {
        if (TryGetTextData(out var thisText) && other.TryGetTextData(out var otherText))
        {
            return thisText.IdenticalTo(otherText, flags);
        }

        return Equals(other);
    }

    public string ToFormattedString(bool rebuildText, bool rebuildAsSource)
    {
        var stringBuilder = new StringBuilder();
        ToFormattedString(rebuildText, rebuildAsSource, stringBuilder);
        return stringBuilder.ToString();
    }

    public void ToFormattedString(bool rebuildText, bool rebuildAsSource, StringBuilder builder)
    {
        Match(
            value => ToFormattedString(value, builder),
            value => ToFormattedString(value, builder),
            value => ToFormattedString(value, builder),
            value => ToFormattedString(value, builder),
            text =>
            {
                if (rebuildText)
                {
                    text.Rebuild();
                }

                builder.Append(rebuildAsSource ? text.BuildSourceString() : text.ToString());
            },
            _ =>
            {
                // Do nothing
            }
        );
    }

    private static void ToFormattedString<T>(T value, StringBuilder builder)
        where T : unmanaged, INumber<T>
    {
        var culture = CultureManager.Instance.CurrentLocale;
        var formattingRules = culture.DecimalNumberFormattingRules;
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        FastDecimalFormat.NumberToString(value, formattingRules, formattingOptions, builder);
    }

    public string ToExportedString()
    {
        var stringBuilder = new StringBuilder();
        ToExportedString(stringBuilder);
        return stringBuilder.ToString();
    }

    public void ToExportedString(StringBuilder builder)
    {
        Match(
            builder,
            (b, x) => b.Append(x),
            (b, x) => b.Append(x).Append('u'),
            (b, x) => b.Append(x).Append('f'),
            (b, x) => b.Append(x),
            (b, x) => TextStringHelper.WriteToBuffer(b, x, true),
            (b, x) => b.WriteScopedEnum("ETextGender::", x)
        );
    }

    public static ParseResult<FormatArg> FromExportedString(ReadOnlySpan<char> str)
    {
        return FromExportedString(new ParseCursor(str));
    }

    internal static ParseResult<FormatArg> FromExportedString(ParseCursor input)
    {
        return input
            .ParseEnum<TextGender>("ETextGender::")
            .Select(Gender)
            .OrElse(i => i.ParseNumber().Select(a => (FormatArg)a))
            .OrElse(i => TextStringHelper.ReadFromBuffer(i, requiresQuotes: true).Select(Text));
    }

    public static implicit operator FormatArg(sbyte value) => Signed(value);

    public static implicit operator FormatArg(short value) => Signed(value);

    public static implicit operator FormatArg(int value) => Signed(value);

    public static implicit operator FormatArg(long value) => Signed(value);

    public static implicit operator FormatArg(byte value) => Unsigned(value);

    public static implicit operator FormatArg(ushort value) => Unsigned(value);

    public static implicit operator FormatArg(uint value) => Unsigned(value);

    public static implicit operator FormatArg(ulong value) => Unsigned(value);

    public static implicit operator FormatArg(float value) => Float(value);

    public static implicit operator FormatArg(double value) => Double(value);

    public static implicit operator FormatArg(Text text) => Text(text);

    public static implicit operator FormatArg(string? str) => Text(str);

    public static implicit operator FormatArg(TextGender gender) => Gender(gender);

    public static implicit operator FormatArg(FormatNumericArg arg)
    {
        return arg.IsDefault ? default : arg.Match(Signed, Unsigned, Float, Double);
    }
}

[Union]
public readonly partial struct FormatNumericArg
{
    internal bool IsDefault => Index == 0;

    [UnionCase]
    public static partial FormatNumericArg Signed(long value);

    [UnionCase]
    public static partial FormatNumericArg Unsigned(ulong value);

    [UnionCase]
    public static partial FormatNumericArg Float(float value);

    [UnionCase]
    public static partial FormatNumericArg Double(double value);

    public static FormatNumericArg FromNumber<T>(T value)
        where T : unmanaged, INumber<T>
    {
        return value switch
        {
            sbyte v => Signed(v),
            short v => Signed(v),
            int v => Signed(v),
            long v => Signed(v),
            byte v => Unsigned(v),
            ushort v => Unsigned(v),
            uint v => Unsigned(v),
            ulong v => Unsigned(v),
            float v => Float(v),
            double v => Double(v),
            _ => throw new ArgumentException($"Cannot convert {value} to a number"),
        };
    }

    public static implicit operator FormatNumericArg(sbyte value) => Signed(value);

    public static implicit operator FormatNumericArg(short value) => Signed(value);

    public static implicit operator FormatNumericArg(int value) => Signed(value);

    public static implicit operator FormatNumericArg(long value) => Signed(value);

    public static implicit operator FormatNumericArg(byte value) => Unsigned(value);

    public static implicit operator FormatNumericArg(ushort value) => Unsigned(value);

    public static implicit operator FormatNumericArg(uint value) => Unsigned(value);

    public static implicit operator FormatNumericArg(ulong value) => Unsigned(value);

    public static implicit operator FormatNumericArg(float value) => Float(value);

    public static implicit operator FormatNumericArg(double value) => Double(value);
}
