// // @file GenderFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.Localization.Formatting;

internal sealed class GenderFormatArgumentModifier : ITextFormatArgumentModifier
{
    private readonly int _longestGenderFormStringLen;
    private readonly bool _doGenderFormsUseFormatArgs;
    private readonly TextFormat _masculineForm;
    private readonly TextFormat _feminineForm;
    private readonly TextFormat _neuterForm;

    public static ITextFormatArgumentModifier? Create(ReadOnlySpan<char> parametersPattern)
    {
        if (ITextFormatArgumentModifier.ParseStringArray(parametersPattern) is not { Length: 2 or 3 } argsValues)
            return null;

        var masculineForm = new TextFormat(argsValues[0]);
        var feminineForm = new TextFormat(argsValues[1]);
        var neuterForm = argsValues.Length == 3 ? new TextFormat(argsValues[2]) : TextFormat.Empty;

        if (!masculineForm.IsValid || !feminineForm.IsValid)
            return null;

        var longestGenderFormStringLen = Math.Max(argsValues[0].Length, argsValues[1].Length);
        if (argsValues.Length == 3)
            longestGenderFormStringLen = Math.Max(longestGenderFormStringLen, argsValues[2].Length);

        var doGenderFormsUseFormatArgs =
            masculineForm.ExpressionType == TextFormat.CompiledExpressionType.Complex
            || feminineForm.ExpressionType == TextFormat.CompiledExpressionType.Complex
            || neuterForm.ExpressionType == TextFormat.CompiledExpressionType.Complex;
        return new GenderFormatArgumentModifier(
            masculineForm,
            feminineForm,
            neuterForm,
            longestGenderFormStringLen,
            doGenderFormsUseFormatArgs
        );
    }

    private GenderFormatArgumentModifier(
        TextFormat masculineForm,
        TextFormat feminineForm,
        TextFormat neuterForm,
        int longestGenderFormStringLen,
        bool doGenderFormsUseFormatArgs
    )
    {
        _longestGenderFormStringLen = longestGenderFormStringLen;
        _doGenderFormsUseFormatArgs = doGenderFormsUseFormatArgs;
        _masculineForm = masculineForm;
        _feminineForm = feminineForm;
        _neuterForm = neuterForm;
    }

    public IEnumerable<string> FormatArgumentNames =>
        _masculineForm
            .FormatArgumentNames.Concat(_feminineForm.FormatArgumentNames)
            .Concat(_neuterForm.FormatArgumentNames);

    public (bool UsesFormatArgs, int Length) EstimateLength()
    {
        return (_doGenderFormsUseFormatArgs, _longestGenderFormStringLen);
    }

    public void Evaluate<TContext>(in FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct
    {
        if (!arg.TryGetGenderData(out var gender))
            return;

        switch (gender)
        {
            case TextGender.Masculine:
                builder.Append(TextFormatter.Format(_masculineForm, in context));
                break;
            case TextGender.Feminine:
                builder.Append(TextFormatter.Format(_feminineForm, in context));
                break;
            case TextGender.Neuter:
                builder.Append(TextFormatter.Format(_neuterForm, in context));
                break;
            default:
                throw new InvalidOperationException("Invalid gender value.");
        }
    }
}
