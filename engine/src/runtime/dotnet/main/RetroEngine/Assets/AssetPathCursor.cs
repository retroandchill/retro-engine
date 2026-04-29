// // @file AssetPathCursor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Assets;

internal readonly ref struct AssetPathCursor
{
    private readonly ref char _start;
    private int NextSeparator { get; init; }
    private readonly int _length;
    private readonly char _separatorChar;

    public ReadOnlySpan<char> FullPath => MemoryMarshal.CreateSpan(ref _start, _length);

    public ReadOnlySpan<char> CurrentPath =>
        NextSeparator == -1 ? FullPath : MemoryMarshal.CreateSpan(ref _start, NextSeparator);

    public ReadOnlySpan<char> RemainingPath => NextSeparator == -1 ? [] : FullPath[(NextSeparator + 1)..];

    public AssetPathCursor(ReadOnlySpan<char> path, char separatorChar)
    {
        _start = ref MemoryMarshal.GetReference(path);
        _length = path.Length;
        _separatorChar = separatorChar;
        NextSeparator = FullPath.IndexOf(_separatorChar);
    }

    public bool TryGetNextChild(out AssetPathCursor next)
    {
        if (NextSeparator == -1)
        {
            next = this;
            return false;
        }

        var nextSeparator = RemainingPath.IndexOf(_separatorChar);
        if (nextSeparator == -1)
        {
            next = this with { NextSeparator = -1 };
            return true;
        }

        next = this with { NextSeparator = NextSeparator + nextSeparator + 1 };
        return true;
    }
}
