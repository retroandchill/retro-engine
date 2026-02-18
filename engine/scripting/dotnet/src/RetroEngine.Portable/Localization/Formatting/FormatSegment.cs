// // @file FormatSegment.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace RetroEngine.Portable.Localization.Formatting;

public abstract record FormatSegment;

public sealed record LiteralSegment(string Text) : FormatSegment;

public sealed record PlaceholderSegment(string Key, ArgModifier? Modifier) : FormatSegment;

public sealed record ArgModifier(string Name, ModifierArgs Args);

public abstract record ModifierArgs;

public sealed record PositionalArgs(IReadOnlyList<string> Values) : ModifierArgs;

public sealed record NamedArgs(IReadOnlyDictionary<string, string> Pairs) : ModifierArgs;
