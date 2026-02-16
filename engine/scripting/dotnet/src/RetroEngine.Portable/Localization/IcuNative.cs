// // @file IcuNative.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization;

internal enum IcuErrorCode : int
{
    ZeroError = 0,
}

internal static partial class IcuNative
{
    private const string IcuLibName = "icuuc";

    [LibraryImport(IcuLibName, EntryPoint = "uloc_getDisplayName")]
    internal static partial int GetDisplayName(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string locale,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string displayLocale,
        Span<char> buffer,
        int bufferLength,
        out IcuErrorCode errorCode
    );
}
