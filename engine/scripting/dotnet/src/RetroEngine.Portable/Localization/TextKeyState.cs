// // @file TextKeyState.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal class TextKeyState
{
    private readonly ReaderWriterLockSlim _lock = new();

    public static TextKeyState State => throw new NotImplementedException();

    public TextKey FindOrAdd(ReadOnlySpan<char> str) => throw new NotImplementedException();

    public string GetStringByIndex(int index) => throw new NotImplementedException();

    public int GetHashByIndex(int index) => throw new NotImplementedException();
}
