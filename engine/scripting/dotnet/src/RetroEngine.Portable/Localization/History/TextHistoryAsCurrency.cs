// // @file TextHistoryAsCurrency.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsCurrency<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    private readonly string? _currencyCode;

    public TextHistoryAsCurrency() { }

    public TextHistoryAsCurrency(
        string displayString,
        T sourceValue,
        string? currencyCode,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture)
    {
        _currencyCode = currencyCode;
    }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? Culture.CurrentCulture;
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = Culture.InvariantCulture;
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }
}
