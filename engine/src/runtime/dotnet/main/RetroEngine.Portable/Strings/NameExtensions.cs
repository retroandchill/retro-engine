// // @file NameExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using Cysharp.Text;

namespace RetroEngine.Portable.Strings;

public static class NameExtensions
{
    public static StringBuilder AppendName(this StringBuilder builder, Name name)
    {
        var maxLength = Encoding.UTF8.GetMaxByteCount(Name.MaxRenderedLength);
        Span<char> buffer = stackalloc char[maxLength];
        var writtenLength = name.WriteUtf16Bytes(buffer);
        return builder.Append(buffer[..writtenLength]);
    }

    public static void AppendName(this ref Utf8ValueStringBuilder builder, Name name)
    {
        Span<byte> buffer = stackalloc byte[Name.MaxRenderedLength];
        var writtenLength = name.WriteUtf8Bytes(buffer);
        builder.AppendLiteral(buffer[..writtenLength]);
    }
}
