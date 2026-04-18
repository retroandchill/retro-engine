// // @file ArchiveMarshalling.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.Utilities;

public static class ArchiveMarshalling
{
    public static void ReadInto<T>(ref ArchiveReader reader, scoped Span<T?> span)
    {
        reader.ReadInto(span);
    }
}
