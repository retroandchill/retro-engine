// // @file PluralFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using RetroEngine.Portable.Collections.Immutable;
using RetroEngine.Portable.Localization.Cultures;
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

    public static ParseResult<ITextFormatArgumentModifier> Create(
        ReadOnlySpan<char> parametersPattern,
        TextPluralType pluralType
    )
    {
        var cursor = new ParseCursor(parametersPattern);
        var argsResult = ITextFormatArgumentModifier.ParseKeyValueArgs(cursor);
        if (!argsResult.HasValue)
            return ParseResult.CastEmpty<ImmutableOrderedDictionary<string, string>, ITextFormatArgumentModifier>(
                argsResult
            );
        var args = argsResult.Value;

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
            return ParseResult.Success<ITextFormatArgumentModifier>(
                new PluralFormatArgumentModifier(
                    pluralType,
                    builder.ToImmutable(),
                    longestPluralFormStringLength,
                    doPluralFormsUseFormatArgs
                ),
                cursor,
                argsResult.Remainder
            );
        }

        return ParseResult.Empty<ITextFormatArgumentModifier>(cursor);
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

        if (!TryGetPluralFormForArgument(in arg, 1, out var valuePluralForm) && arg.TryGetTextData(out var textValue))
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
            var form = a.Match(
                TextPluralForm? (val) => culture.GetPluralForm(val * argValueMultiplier, _pluralType),
                val => culture.GetPluralForm(val * (uint)argValueMultiplier, _pluralType),
                val => culture.GetPluralForm(val * argValueMultiplier, _pluralType),
                val => culture.GetPluralForm(val * argValueMultiplier, _pluralType),
                _ => null,
                _ => null
            );

            pluralForm = form ?? default;
            return form is not null;
        }
    }
}
