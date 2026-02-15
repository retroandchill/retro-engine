// // @file ITextData.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal interface ITextData
{
    string SourceString { get; }

    string DisplayString { get; }

    string? LocalizedString { get; }

    ushort GlobalHistoryRevision { get; }

    ushort LocalHistoryRevision { get; }

    TextHistory History { get; }
}
