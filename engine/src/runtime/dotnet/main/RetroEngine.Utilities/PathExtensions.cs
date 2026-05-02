// // @file PathExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Utilities.Collections;

namespace RetroEngine.Utilities;

public static class PathExtensions
{
    private static readonly ImmutableArray<char> PortableInvalidCharacters =
    [
        .. Path.GetInvalidFileNameChars().Union(['/', '\\', '<', '>', ':', '"', '|', '?', '*']).Distinct(),
    ];

    private static readonly ImmutableArray<string> ReservedNames =
    [
        "CON",
        "PRN",
        "AUX",
        "NUL",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9",
    ];

    extension(Path)
    {
        public static bool IsValidPortableFileName(ReadOnlySpan<char> fileName)
        {
            if (fileName.IsWhiteSpace() || fileName.Length > 255)
                return false;

            if (fileName.IndexOfAny(PortableInvalidCharacters.AsSpan()) != -1)
                return false;

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (ReservedNames.Contains(nameWithoutExtension))
                return false;

            return !fileName.EndsWith('.') && !fileName.EndsWith(' ');
        }
    }
}
