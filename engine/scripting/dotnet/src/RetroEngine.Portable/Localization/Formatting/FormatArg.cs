// // @file FormatArgumentValue.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using Dusharp;

namespace RetroEngine.Portable.Localization.Formatting;

public enum TextGender : byte
{
    Masculine,
    Feminine,
    Neuter,
}

[Union]
public partial struct FormatArg
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
        throw new NotImplementedException();
    }

    public void ToFormattedString(bool rebuildText, bool rebuildAsSource, StringBuilder builder)
    {
        throw new NotImplementedException();
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
}
