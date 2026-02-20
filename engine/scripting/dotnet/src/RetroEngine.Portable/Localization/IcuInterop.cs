// // @file IcuInteropBridge.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization;

internal enum IcuErrorCode
{
    ZeroError = 0,
}

public static partial class IcuInterop
{
    public static Span<char> GetDisplayName(
        ReadOnlySpan<byte> targetLocal,
        ReadOnlySpan<byte> displayLocale,
        Span<char> result
    )
    {
        var correctLength = NativeGetDisplayName(targetLocal, displayLocale, result, result.Length, out _);
        return result[..correctLength];
    }

    [LibraryImport("icuuc", EntryPoint = "uloc_getDisplayName")]
    private static partial int NativeGetDisplayName(
        ReadOnlySpan<byte> targetLocal,
        ReadOnlySpan<byte> displayLocale,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );
}
