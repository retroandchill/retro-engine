// // @file LocalizedString.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

[Flags]
public enum LocalizedStringFlags : byte
{
    None,
    Transient = 1 << 0,
    CultureInvariant = 1 << 1,
}

public readonly record struct TextRevision(ushort Global, ushort Local);

public interface ILocalizedString
{
    string SourceString { get; }

    string DisplayString { get; }

    TextRevision Revision { get; }

    TextId TextId => TextId.Empty;

    LocalizedStringFlags Flags { get; }

    public static ILocalizedString CreateUnlocalized(
        string sourceString,
        LocalizedStringFlags flags = LocalizedStringFlags.None
    )
    {
        return new UnlocalizedString(sourceString, flags);
    }
}
