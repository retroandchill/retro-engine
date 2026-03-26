// // @file CharSet.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace RetroEngine.Portable.Localization.Exporting;

public unsafe struct ParsedChar
{
    private fixed char _data[2];
    private readonly int _length;

    public ParsedChar(char character)
    {
        _data[0] = character;
        _length = 1;
    }

    public ParsedChar(char character1, char character2)
    {
        _data[0] = character1;
        _data[1] = character2;
        _length = 2;
    }

    public ReadOnlySpan<char> Data
    {
        get
        {
            fixed (char* dataPtr = _data)
            {
                return new ReadOnlySpan<char>(dataPtr, _length);
            }
        }
    }

    public static implicit operator ParsedChar(char character) => new(character);
}
