// // @file IStructuredSerializer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace RetroEngine.Portable.Serialization;

public interface IStructuredSerializer
{
    void Serialize<T>(Stream stream, T value);
    ValueTask SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default);

    void Serialize<T>(IBufferWriter<byte> writer, T value);
    ValueTask SerializeAsync<T>(IBufferWriter<byte> writer, T value, CancellationToken cancellationToken = default);

    T Deserialize<T>(Stream stream);
    ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);

    T Deserialize<T>(ReadOnlySpan<byte> bytes);
}
