// // @file ITextFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using RetroEngine.Portable.Collections;
using RetroEngine.Portable.Collections.Immutable;
using Superpower;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

public interface ITextFormatArgumentModifier
{
    string ModifierPattern { get; }

    (bool UsesFormatArgs, int Length) EstimateLength();

    void Evaluate<TContext>(FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct;

    protected static ImmutableOrderedDictionary<string, string>? ParseKeyValueArgs(string argsString)
    {
        var result = KeyValueArgsParser.TryParse(argsString);
        return result.HasValue ? result.Value : null;
    }

    protected static ImmutableArray<string>? ParseStringArray(string argsString)
    {
        var result = StringArrayParser.TryParse(argsString);
        return result.HasValue ? result.Value : null;
    }

    private static readonly TextParser<ImmutableOrderedDictionary<string, string>> KeyValueArgsParser =
        TextFormatParsingUtils
            .KeyValueArg.Between(TextFormatParsingUtils.Whitespace, TextFormatParsingUtils.Whitespace)
            .ManyDelimitedBy(TextFormatParsingUtils.Comma)
            .Select(kv => kv.ToImmutableOrderedDictionary());

    private static readonly TextParser<ImmutableArray<string>> StringArrayParser = TextFormatParsingUtils
        .ArgValue.ManyDelimitedBy(TextFormatParsingUtils.Comma)
        .Select(x => x.ToImmutableArray());
}

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

[InlineArray(6)]
internal struct PluralFormsArray
{
    public TextFormat Format0;
}

internal sealed class PluralFormatArgumentModifier : ITextFormatArgumentModifier
{
    public string ModifierPattern { get; }
    private readonly TextPluralType _pluralType;
    private readonly int _longestPluralFormStringLength;
    private readonly bool _doPluralFormsUseFormatArgs;
    private readonly PluralFormsArray _pluralForms;

    public static ITextFormatArgumentModifier? Create(
        string modifierPattern,
        TextPluralType pluralType,
        string argsString
    )
    {
        var args = ITextFormatArgumentModifier.ParseKeyValueArgs(argsString);
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

        return builder.Count == args.Count
            ? new PluralFormatArgumentModifier(
                modifierPattern,
                pluralType,
                builder.ToImmutable(),
                longestPluralFormStringLength,
                doPluralFormsUseFormatArgs
            )
            : null;
    }

    private PluralFormatArgumentModifier(
        string modifierPattern,
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

        ModifierPattern = modifierPattern;
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

    public void Evaluate<TContext>(FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct
    {
        var culture = LocalizationManager.Instance.CurrentCulture;

        if (!TryGetPluralFormForArgument(arg, 1, out var valuePluralForm) && arg.TryGetTextData(out var textValue))
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

        bool TryGetPluralFormForArgument(FormatArg a, int argValueMultiplier, out TextPluralForm pluralForm)
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
