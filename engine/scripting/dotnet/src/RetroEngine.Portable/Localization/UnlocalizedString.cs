// // @file UnlocalizedString.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal sealed class UnlocalizedString(string sourceString, LocalizedStringFlags flags) : ILocalizedString
{
    public string SourceString { get; } = sourceString;
    public string DisplayString => SourceString;
    public TextRevision Revision => new(0, 0);
    public LocalizedStringFlags Flags { get; } = flags;
}
