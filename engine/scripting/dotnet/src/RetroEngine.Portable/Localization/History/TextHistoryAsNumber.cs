// // @file TextHistoryAsNumber.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsNumber<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    public TextHistoryAsNumber() { }

    public TextHistoryAsNumber(
        string displayString,
        T sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture) { }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? Culture.CurrentCulture;
        var formattingRules = culture.DecimalNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = Culture.InvariantCulture;
        var formattingRules = culture.DecimalNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules);
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Number, FormatNumericArg.FromNumber(SourceValue));
    }
}
