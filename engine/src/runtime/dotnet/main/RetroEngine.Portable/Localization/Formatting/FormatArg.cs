// // @file FormatArgumentValue.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using MessagePack;
using MessagePack.Formatters;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Utilities;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

public enum TextGender : byte
{
    Masculine,
    Feminine,
    Neuter,
}

public enum FormatArgumentType : byte
{
    Text,
    Int,
    UInt,
    Float,
    Double,
    Gender,
}

[MessagePackFormatter(typeof(MessagePackFormatter))]
public readonly struct FormatArg : IEquatable<FormatArg>, IEqualityOperators<FormatArg, FormatArg, bool>
{
    public FormatArgumentType Type { get; }

    [StructLayout(LayoutKind.Explicit)]
    private struct NumericData
    {
        [FieldOffset(0)]
        public long IntValue;

        [FieldOffset(0)]
        public ulong UIntValue;

        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public double DoubleValue;

        [FieldOffset(0)]
        public TextGender GenderValue;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly NumericData _blittableData;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Text _textData;

    public FormatArg(sbyte value)
    {
        Type = FormatArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatArg(byte value)
    {
        Type = FormatArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatArg(short value)
    {
        Type = FormatArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatArg(ushort value)
    {
        Type = FormatArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatArg(int value)
    {
        Type = FormatArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatArg(uint value)
    {
        Type = FormatArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatArg(long value)
    {
        Type = FormatArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatArg(ulong value)
    {
        Type = FormatArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatArg(float value)
    {
        Type = FormatArgumentType.Float;
        _blittableData.FloatValue = value;
    }

    public FormatArg(double value)
    {
        Type = FormatArgumentType.Double;
        _blittableData.DoubleValue = value;
    }

    public FormatArg(Text text)
    {
        Type = FormatArgumentType.Text;
        _textData = text;
    }

    public FormatArg(TextGender gender)
    {
        Type = FormatArgumentType.Gender;
        _blittableData.GenderValue = gender;
    }

    public bool TryGetValue(out Text text)
    {
        if (Type == FormatArgumentType.Text)
        {
            text = _textData;
            return true;
        }

        text = default;
        return false;
    }

    public bool TryGetValue(out long value)
    {
        if (Type == FormatArgumentType.Int)
        {
            value = _blittableData.IntValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ulong value)
    {
        if (Type == FormatArgumentType.UInt)
        {
            value = _blittableData.UIntValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out float value)
    {
        if (Type == FormatArgumentType.Float)
        {
            value = _blittableData.FloatValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (Type == FormatArgumentType.Double)
        {
            value = _blittableData.DoubleValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out TextGender gender)
    {
        if (Type == FormatArgumentType.Gender)
        {
            gender = _blittableData.GenderValue;
            return true;
        }

        gender = default;
        return false;
    }

    public bool IdenticalTo(FormatArg other, TextIdenticalModeFlags flags)
    {
        if (TryGetValue(out Text thisText) && other.TryGetValue(out Text otherText))
        {
            return thisText.IdenticalTo(otherText, flags);
        }

        return Equals(other);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is FormatArg other && Equals(other);
    }

    public bool Equals(FormatArg other)
    {
        if (Type != other.Type)
            return false;

        return Type switch
        {
            FormatArgumentType.Text => _textData == other._textData,
            FormatArgumentType.Int => _blittableData.IntValue == other._blittableData.IntValue,
            FormatArgumentType.UInt => _blittableData.UIntValue == other._blittableData.UIntValue,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            FormatArgumentType.Float => _blittableData.FloatValue == other._blittableData.FloatValue,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            FormatArgumentType.Double => _blittableData.DoubleValue == other._blittableData.DoubleValue,
            FormatArgumentType.Gender => _blittableData.GenderValue == other._blittableData.GenderValue,
            _ => throw new InvalidOperationException("Invalid format argument type."),
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            FormatArgumentType.Text => HashCode.Combine(FormatArgumentType.Text, _textData),
            FormatArgumentType.Int => HashCode.Combine(FormatArgumentType.Int, _blittableData.IntValue),
            FormatArgumentType.UInt => HashCode.Combine(FormatArgumentType.UInt, _blittableData.UIntValue),
            FormatArgumentType.Float => HashCode.Combine(FormatArgumentType.Float, _blittableData.FloatValue),
            FormatArgumentType.Double => HashCode.Combine(FormatArgumentType.Double, _blittableData.DoubleValue),
            FormatArgumentType.Gender => HashCode.Combine(FormatArgumentType.Gender, _blittableData.GenderValue),
            _ => throw new InvalidOperationException("Invalid format argument type."),
        };
    }

    public static bool operator ==(FormatArg left, FormatArg right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FormatArg left, FormatArg right)
    {
        return !(left == right);
    }

    public string ToFormattedString(bool rebuildText, bool rebuildAsSource)
    {
        var stringBuilder = new StringBuilder();
        ToFormattedString(rebuildText, rebuildAsSource, stringBuilder);
        return stringBuilder.ToString();
    }

    public void ToFormattedString(bool rebuildText, bool rebuildAsSource, StringBuilder builder)
    {
        switch (Type)
        {
            case FormatArgumentType.Text:
                if (rebuildText)
                {
                    _textData.Rebuild();
                }

                builder.Append(rebuildAsSource ? _textData.BuildSourceString() : _textData.ToString());
                break;
            case FormatArgumentType.Int:
                ToFormattedString(_blittableData.IntValue, builder);
                break;
            case FormatArgumentType.UInt:
                ToFormattedString(_blittableData.UIntValue, builder);
                break;
            case FormatArgumentType.Float:
                ToFormattedString(_blittableData.FloatValue, builder);
                break;
            case FormatArgumentType.Double:
                ToFormattedString(_blittableData.DoubleValue, builder);
                break;
            case FormatArgumentType.Gender:
                // Do nothing
                break;
            default:
                throw new InvalidOperationException("Invalid format argument type.");
        }
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
        switch (Type)
        {
            case FormatArgumentType.Text:
                TextStringifier.ExportToString(builder, _textData, true);
                break;
            case FormatArgumentType.Int:
                builder.Append(_blittableData.IntValue);
                break;
            case FormatArgumentType.UInt:
                builder.Append(_blittableData.UIntValue).Append('u');
                break;
            case FormatArgumentType.Float:
                builder.Append(_blittableData.FloatValue).Append('f');
                break;
            case FormatArgumentType.Double:
                builder.Append(_blittableData.DoubleValue);
                break;
            case FormatArgumentType.Gender:
                builder.WriteScopedEnum("ETextGender::", _blittableData.GenderValue);
                break;
            default:
                throw new InvalidOperationException("Invalid format argument type.");
        }
    }

    public static FormatArg FromExportedString(ReadOnlySpan<char> str)
    {
        return Parser.Parse(str);
    }

    internal static TextParser<FormatArg> Parser { get; } =
        Symbols
            .EnumLiteral<TextGender>("ETextGender::")
            .Select(g => new FormatArg(g))
            .Or(
                TextStringReader.Number.Select(a => (FormatArg)a),
                TextStringReader.QuotedText.Select(t => new FormatArg(t))
            );

    public static implicit operator FormatArg(sbyte value) => new(value);

    public static implicit operator FormatArg(short value) => new(value);

    public static implicit operator FormatArg(int value) => new(value);

    public static implicit operator FormatArg(long value) => new(value);

    public static implicit operator FormatArg(byte value) => new(value);

    public static implicit operator FormatArg(ushort value) => new(value);

    public static implicit operator FormatArg(uint value) => new(value);

    public static implicit operator FormatArg(ulong value) => new(value);

    public static implicit operator FormatArg(float value) => new(value);

    public static implicit operator FormatArg(double value) => new(value);

    public static implicit operator FormatArg(Text text) => new(text);

    public static implicit operator FormatArg(string? str) => new(str);

    public static implicit operator FormatArg(TextGender gender) => new(gender);

    public static implicit operator FormatArg(FormatNumericArg arg)
    {
        if (arg.TryGetValue(out long value))
        {
            return new FormatArg(value);
        }

        if (arg.TryGetValue(out ulong value2))
        {
            return new FormatArg(value2);
        }

        if (arg.TryGetValue(out float value3))
        {
            return new FormatArg(value3);
        }

        return arg.TryGetValue(out double value4)
            ? new FormatArg(value4)
            : throw new ArgumentException($"Cannot convert {arg} to a number");
    }

    public sealed class MessagePackFormatter : IMessagePackFormatter<FormatArg>
    {
        public void Serialize(ref MessagePackWriter writer, FormatArg value, MessagePackSerializerOptions options)
        {
            var formatterResolver = options.Resolver;
            formatterResolver.GetFormatterWithVerify<FormatArgumentType>().Serialize(ref writer, value.Type, options);

            switch (value.Type)
            {
                case FormatArgumentType.Text:
                    formatterResolver.GetFormatterWithVerify<Text>().Serialize(ref writer, value._textData, options);
                    break;
                case FormatArgumentType.Int:
                    writer.Write(value._blittableData.IntValue);
                    break;
                case FormatArgumentType.UInt:
                    writer.Write(value._blittableData.UIntValue);
                    break;
                case FormatArgumentType.Float:
                    writer.Write(value._blittableData.FloatValue);
                    break;
                case FormatArgumentType.Double:
                    writer.Write(value._blittableData.DoubleValue);
                    break;
                case FormatArgumentType.Gender:
                    formatterResolver
                        .GetFormatterWithVerify<TextGender>()
                        .Serialize(ref writer, value._blittableData.GenderValue, options);
                    break;
                default:
                    throw new InvalidOperationException("Invalid format argument type.");
            }
        }

        public FormatArg Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var formatterResolver = options.Resolver;
            var type = formatterResolver.GetFormatterWithVerify<FormatArgumentType>().Deserialize(ref reader, options);

            return type switch
            {
                FormatArgumentType.Text => formatterResolver
                    .GetFormatterWithVerify<Text>()
                    .Deserialize(ref reader, options),
                FormatArgumentType.Int => reader.ReadInt64(),
                FormatArgumentType.UInt => reader.ReadUInt64(),
                FormatArgumentType.Float => reader.ReadSingle(),
                FormatArgumentType.Double => reader.ReadDouble(),
                FormatArgumentType.Gender => formatterResolver
                    .GetFormatterWithVerify<TextGender>()
                    .Deserialize(ref reader, options),
                _ => throw new InvalidOperationException("Invalid format argument type."),
            };
        }
    }
}

public enum FormatNumericArgumentType : byte
{
    Int,
    UInt,
    Float,
    Double,
}

[MessagePackFormatter(typeof(MessagePackFormatter))]
public readonly struct FormatNumericArg
    : IEquatable<FormatNumericArg>,
        IEqualityOperators<FormatNumericArg, FormatNumericArg, bool>
{
    public FormatNumericArgumentType Type { get; }

    [StructLayout(LayoutKind.Explicit)]
    private struct NumericData
    {
        [FieldOffset(0)]
        public long IntValue;

        [FieldOffset(0)]
        public ulong UIntValue;

        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public double DoubleValue;
    }

    private readonly NumericData _blittableData;

    public FormatNumericArg(sbyte value)
    {
        Type = FormatNumericArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatNumericArg(byte value)
    {
        Type = FormatNumericArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatNumericArg(short value)
    {
        Type = FormatNumericArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatNumericArg(ushort value)
    {
        Type = FormatNumericArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatNumericArg(int value)
    {
        Type = FormatNumericArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatNumericArg(uint value)
    {
        Type = FormatNumericArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatNumericArg(long value)
    {
        Type = FormatNumericArgumentType.Int;
        _blittableData.IntValue = value;
    }

    public FormatNumericArg(ulong value)
    {
        Type = FormatNumericArgumentType.UInt;
        _blittableData.UIntValue = value;
    }

    public FormatNumericArg(float value)
    {
        Type = FormatNumericArgumentType.Float;
        _blittableData.FloatValue = value;
    }

    public FormatNumericArg(double value)
    {
        Type = FormatNumericArgumentType.Double;
        _blittableData.DoubleValue = value;
    }

    public static FormatNumericArg FromNumber<T>(T value)
        where T : unmanaged, INumber<T>
    {
        return value switch
        {
            sbyte v => new FormatNumericArg(v),
            short v => new FormatNumericArg(v),
            int v => new FormatNumericArg(v),
            long v => new FormatNumericArg(v),
            byte v => new FormatNumericArg(v),
            ushort v => new FormatNumericArg(v),
            uint v => new FormatNumericArg(v),
            ulong v => new FormatNumericArg(v),
            float v => new FormatNumericArg(v),
            double v => new FormatNumericArg(v),
            _ => throw new ArgumentException($"Cannot convert {value} to a number"),
        };
    }

    public bool TryGetValue(out long value)
    {
        if (Type == FormatNumericArgumentType.Int)
        {
            value = _blittableData.IntValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ulong value)
    {
        if (Type == FormatNumericArgumentType.UInt)
        {
            value = _blittableData.UIntValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out float value)
    {
        if (Type == FormatNumericArgumentType.Float)
        {
            value = _blittableData.FloatValue;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (Type == FormatNumericArgumentType.Double)
        {
            value = _blittableData.DoubleValue;
            return true;
        }

        value = 0;
        return false;
    }

    public double ToDouble()
    {
        return Type switch
        {
            FormatNumericArgumentType.Int => _blittableData.IntValue,
            FormatNumericArgumentType.UInt => _blittableData.UIntValue,
            FormatNumericArgumentType.Float => _blittableData.FloatValue,
            FormatNumericArgumentType.Double => _blittableData.DoubleValue,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static implicit operator FormatNumericArg(sbyte value) => new(value);

    public static implicit operator FormatNumericArg(short value) => new(value);

    public static implicit operator FormatNumericArg(int value) => new(value);

    public static implicit operator FormatNumericArg(long value) => new(value);

    public static implicit operator FormatNumericArg(byte value) => new(value);

    public static implicit operator FormatNumericArg(ushort value) => new(value);

    public static implicit operator FormatNumericArg(uint value) => new(value);

    public static implicit operator FormatNumericArg(ulong value) => new(value);

    public static implicit operator FormatNumericArg(float value) => new(value);

    public static implicit operator FormatNumericArg(double value) => new(value);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is FormatNumericArg other && Equals(other);
    }

    public bool Equals(FormatNumericArg other)
    {
        if (Type != other.Type)
            return false;

        return Type switch
        {
            FormatNumericArgumentType.Int => _blittableData.IntValue == other._blittableData.IntValue,
            FormatNumericArgumentType.UInt => _blittableData.UIntValue == other._blittableData.UIntValue,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            FormatNumericArgumentType.Float => _blittableData.FloatValue == other._blittableData.FloatValue,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            FormatNumericArgumentType.Double => _blittableData.DoubleValue == other._blittableData.DoubleValue,
            _ => throw new InvalidOperationException("Invalid format argument type."),
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            FormatNumericArgumentType.Int => HashCode.Combine(FormatNumericArgumentType.Int, _blittableData.IntValue),
            FormatNumericArgumentType.UInt => HashCode.Combine(
                FormatNumericArgumentType.UInt,
                _blittableData.UIntValue
            ),
            FormatNumericArgumentType.Float => HashCode.Combine(
                FormatNumericArgumentType.Float,
                _blittableData.FloatValue
            ),
            FormatNumericArgumentType.Double => HashCode.Combine(
                FormatNumericArgumentType.Double,
                _blittableData.DoubleValue
            ),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static bool operator ==(FormatNumericArg left, FormatNumericArg right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FormatNumericArg left, FormatNumericArg right)
    {
        return !(left == right);
    }

    public sealed class MessagePackFormatter : IMessagePackFormatter<FormatNumericArg>
    {
        public void Serialize(
            ref MessagePackWriter writer,
            FormatNumericArg value,
            MessagePackSerializerOptions options
        )
        {
            var formatterResolver = options.Resolver;
            formatterResolver
                .GetFormatterWithVerify<FormatNumericArgumentType>()
                .Serialize(ref writer, value.Type, options);

            switch (value.Type)
            {
                case FormatNumericArgumentType.Int:
                    writer.Write(value._blittableData.IntValue);
                    break;
                case FormatNumericArgumentType.UInt:
                    writer.Write(value._blittableData.UIntValue);
                    break;
                case FormatNumericArgumentType.Float:
                    writer.Write(value._blittableData.FloatValue);
                    break;
                case FormatNumericArgumentType.Double:
                    writer.Write(value._blittableData.DoubleValue);
                    break;
                default:
                    throw new InvalidOperationException("Invalid format argument type.");
            }
        }

        public FormatNumericArg Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var formatterResolver = options.Resolver;
            var type = formatterResolver
                .GetFormatterWithVerify<FormatNumericArgumentType>()
                .Deserialize(ref reader, options);

            return type switch
            {
                FormatNumericArgumentType.Int => reader.ReadInt64(),
                FormatNumericArgumentType.UInt => reader.ReadUInt64(),
                FormatNumericArgumentType.Float => reader.ReadSingle(),
                FormatNumericArgumentType.Double => reader.ReadDouble(),
                _ => throw new InvalidOperationException("Invalid format argument type."),
            };
        }
    }
}
