// // @file LocalizedString.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.History;

namespace RetroEngine.Portable.Localization;

public readonly record struct TextRevisions(ushort Global, ushort Local);

internal interface ITextData
{
    string SourceString { get; }

    string DisplayString { get; }

    string? LocalizedString { get; }

    TextRevisions Revisions { get; }

    TextHistory History { get; }
}
