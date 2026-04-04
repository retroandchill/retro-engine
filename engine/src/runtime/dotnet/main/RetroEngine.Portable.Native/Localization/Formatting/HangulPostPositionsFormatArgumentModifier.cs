// // @file HangulPostPositionsFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.Localization.Formatting;

internal sealed class HangulPostPositionsFormatArgumentModifier : ITextFormatArgumentModifier
{
    private enum SuffixMode
    {
        ConsonantOrVowel,
        ConsonantNotRieulOrVowel,
    }

    private readonly string _consonantSuffix;
    private readonly string _vowelSuffix;
    private readonly SuffixMode _suffixMode;

    public static ITextFormatArgumentModifier? Create(ReadOnlySpan<char> input)
    {
        return ITextFormatArgumentModifier.ParseStringArray(input) is { Length: 2 } argsValues
            ? new HangulPostPositionsFormatArgumentModifier(argsValues[0], argsValues[1])
            : null;
    }

    private HangulPostPositionsFormatArgumentModifier(string consonantSuffix, string vowelSuffix)
    {
        _consonantSuffix = consonantSuffix;
        _vowelSuffix = vowelSuffix;

        if (
            _consonantSuffix.Equals("으로", StringComparison.Ordinal)
            && _vowelSuffix.Equals("\uB85C", StringComparison.Ordinal)
        )
        {
            _suffixMode = SuffixMode.ConsonantNotRieulOrVowel;
        }
        else
        {
            _suffixMode = SuffixMode.ConsonantOrVowel;
        }
    }

    public (bool UsesFormatArgs, int Length) EstimateLength()
    {
        return (false, 2);
    }

    public IEnumerable<string> FormatArgumentNames => [];

    public void Evaluate<TContext>(in FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct
    {
        var startPos = builder.Length;
        TextFormatter.ArgumentValueToFormattedString(in arg, in context, builder);
        var endPos = builder.Length;

        if (startPos == endPos)
            return;

        var lastArgChar = builder[endPos - 1];
        if ((lastArgChar < 0xAC00 || lastArgChar > 0xD7A3) && !char.IsAsciiDigit(lastArgChar))
            return;

        var isConsonant = (lastArgChar - 0xAC00) % 28 != 0 || lastArgChar is '0' or '1' or '3' or '6' or '7' or '8';
        if (isConsonant && _suffixMode == SuffixMode.ConsonantNotRieulOrVowel)
        {
            var isRieul = ((lastArgChar - 0xAC00) % 28 == 8) || lastArgChar is '1' or '7' or '8';
            if (isRieul)
                isConsonant = false;
        }

        builder.Append(isConsonant ? _consonantSuffix : _vowelSuffix);
    }
}
