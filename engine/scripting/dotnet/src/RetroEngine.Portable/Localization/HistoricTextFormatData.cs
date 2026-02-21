// // @file HistoricTextNumericData.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Collections.Immutable;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public sealed record HistoricTextFormatData(
    Text FormattedText,
    TextFormat SourceFormat,
    ImmutableDictionary<string, FormatArg> Args
);

public readonly record struct HistoricTextNumericData(NumberFormatType FormatType, FormatNumericArg SourceValue)
{
    public HistoricTextNumericData()
        : this(NumberFormatType.Number, FormatNumericArg.Signed(0)) { }
}
