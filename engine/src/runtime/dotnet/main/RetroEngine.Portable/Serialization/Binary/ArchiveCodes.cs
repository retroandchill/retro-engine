// // @file ArchiveCodes.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Serialization.Binary;

public static class ArchiveCodes
{
    public const int NullCollection = -1;

    public const byte WideTag = 250; // for Union, 250 is wide tag
    public const byte ReferenceId = 250; // for CircularReference, 250 is referenceId marker, next VarInt id reference.

    public const byte Reserved1 = 250;
    public const byte Reserved2 = 251;
    public const byte Reserved3 = 252;
    public const byte Reserved4 = 253;
    public const byte Reserved5 = 254;
    public const byte NullObject = 255;
}
