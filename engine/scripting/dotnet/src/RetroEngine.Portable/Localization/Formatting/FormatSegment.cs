// // @file FormatSegment.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Formatting;

public abstract record FormatSegment;

public sealed record LiteralSegment(string Text) : FormatSegment;

public sealed record PlaceholderSegment(string Key, ITextFormatArgumentModifier? Modifier) : FormatSegment;
