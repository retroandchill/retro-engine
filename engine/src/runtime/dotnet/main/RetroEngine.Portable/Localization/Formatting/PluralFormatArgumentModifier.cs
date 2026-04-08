// // @file PluralFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Utilities.Collections.Immutable;
using ZParse;

namespace RetroEngine.Portable.Localization.Formatting;

public enum TextPluralType : byte
{
    Cardinal,
    Ordinal,
}

public enum TextPluralForm : byte
{
    Zero,
    One,
    Two,
    Few,
    Many,
    Other,
}

[InlineArray(Size)]
internal struct PluralFormsArray
{
    public const int Size = 6;

    public TextFormat Format0;
}

internal sealed class PluralFormatArgumentModifier : ITextFormatArgumentModifier
{
    private readonly TextPluralType _pluralType;
    private readonly int _longestPluralFormStringLength;
    private readonly bool _doPluralFormsUseFormatArgs;
    private readonly PluralFormsArray _pluralForms;

    public static ITextFormatArgumentModifier? Create(ReadOnlySpan<char> parametersPattern, TextPluralType pluralType)
    {
        var cursor = new TextSegment(parametersPattern);
        var args = ITextFormatArgumentModifier.ParseKeyValueArgs(cursor);
        if (args is null)
            return null;

        var doPluralFormsUseFormatArgs = false;
        var longestPluralFormStringLength = 0;
        var builder = ImmutableOrderedDictionary.CreateBuilder<string, TextFormat>(args.Count);
        foreach (var (key, value) in args)
        {
            var textFormat = new TextFormat(value);
            if (!textFormat.IsValid)
                break;

            doPluralFormsUseFormatArgs |= textFormat.ExpressionType == TextFormat.CompiledExpressionType.Complex;
            longestPluralFormStringLength = Math.Max(longestPluralFormStringLength, value.Length);

            builder.Add(key, textFormat);
        }

        if (builder.Count == args.Count)
        {
            return new PluralFormatArgumentModifier(
                pluralType,
                builder.ToImmutable(),
                longestPluralFormStringLength,
                doPluralFormsUseFormatArgs
            );
        }

        return null;
    }

    private PluralFormatArgumentModifier(
        TextPluralType pluralType,
        IReadOnlyDictionary<string, TextFormat> pluralForms,
        int longestPluralFormStringLength,
        bool doPluralFormsUseFormatArgs
    )
    {
        const string zeroString = "zero";
        const string oneString = "one";
        const string twoString = "two";
        const string fewString = "few";
        const string manyString = "many";
        const string otherString = "other";

        _pluralType = pluralType;
        _longestPluralFormStringLength = longestPluralFormStringLength;
        _doPluralFormsUseFormatArgs = doPluralFormsUseFormatArgs;

        _pluralForms[(int)TextPluralForm.Zero] = pluralForms.GetValueOrDefault(zeroString, TextFormat.Empty);
        _pluralForms[(int)TextPluralForm.One] = pluralForms.GetValueOrDefault(oneString, TextFormat.Empty);
        _pluralForms[(int)TextPluralForm.Two] = pluralForms.GetValueOrDefault(twoString, TextFormat.Empty);
        _pluralForms[(int)TextPluralForm.Few] = pluralForms.GetValueOrDefault(fewString, TextFormat.Empty);
        _pluralForms[(int)TextPluralForm.Many] = pluralForms.GetValueOrDefault(manyString, TextFormat.Empty);
        _pluralForms[(int)TextPluralForm.Other] = pluralForms.GetValueOrDefault(otherString, TextFormat.Empty);
    }

    public (bool UsesFormatArgs, int Length) EstimateLength()
    {
        return (_doPluralFormsUseFormatArgs, _longestPluralFormStringLength);
    }

    public IEnumerable<string> FormatArgumentNames
    {
        get
        {
            return Enumerable
                .Range(0, PluralFormsArray.Size)
                .SelectMany(i => _pluralForms[i].FormatArgumentNames)
                .Distinct();
        }
    }

    public void Evaluate<TContext>(in FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct
    {
        var culture = CultureManager.Instance.CurrentLocale;

        if (!TryGetPluralFormForArgument(in arg, 1, out var valuePluralForm) && arg.TryGetValue(out Text textValue))
        {
            var textValueNumericData = textValue.HistoricNumericData;
            if (textValueNumericData is not null)
            {
                TryGetPluralFormForArgument(
                    textValueNumericData.Value.SourceValue,
                    textValueNumericData.Value.FormatType == NumberFormatType.Percent ? 100 : 1,
                    out valuePluralForm
                );
            }
        }

        builder.Append(TextFormatter.Format(_pluralForms[(int)valuePluralForm], context));

        return;

        bool TryGetPluralFormForArgument(in FormatArg a, int argValueMultiplier, out TextPluralForm pluralForm)
        {
            if (a.TryGetValue(out long longValue))
            {
                pluralForm = culture.GetPluralForm(longValue * argValueMultiplier, _pluralType);
                return true;
            }

            if (a.TryGetValue(out ulong ulongValue))
            {
                pluralForm = culture.GetPluralForm(ulongValue * (ulong)argValueMultiplier, _pluralType);
                return true;
            }

            if (a.TryGetValue(out float floatValue))
            {
                pluralForm = culture.GetPluralForm(floatValue * argValueMultiplier, _pluralType);
                return true;
            }

            if (a.TryGetValue(out double doubleValue))
            {
                pluralForm = culture.GetPluralForm(doubleValue * argValueMultiplier, _pluralType);
                return true;
            }

            pluralForm = default;
            return false;
        }
    }
}
