// // @file ArchiveSerializationException.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Serialization.Binary;

public sealed class ArchiveSerializationException : Exception
{
    public ArchiveSerializationException(string message)
        : base(message) { }

    public ArchiveSerializationException(string message, Exception innerException)
        : base(message, innerException) { }
}
