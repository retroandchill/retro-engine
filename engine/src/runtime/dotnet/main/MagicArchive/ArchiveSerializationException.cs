// // @file ArchiveSerializationException.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MagicArchive;

public sealed class ArchiveSerializationException : Exception
{
    public ArchiveSerializationException(string message)
        : base(message) { }

    public ArchiveSerializationException(string message, Exception innerException)
        : base(message, innerException) { }

    [DoesNotReturn]
    public static void ThrowMessage(string message)
    {
        throw new ArchiveSerializationException(message);
    }

    [DoesNotReturn]
    public static void ThrowInvalidPropertyCount(byte expected, byte actual)
    {
        throw new ArchiveSerializationException(
            $"Current object's property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning."
        );
    }

    [DoesNotReturn]
    public static void ThrowInvalidPropertyCount(Type type, byte expected, byte actual)
    {
        throw new ArchiveSerializationException(
            $"{type.FullName} property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning."
        );
    }

    [DoesNotReturn]
    public static void ThrowInvalidCollection()
    {
        throw new ArchiveSerializationException($"Current read to collection, the buffer header is not collection.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidRange(int expected, int actual)
    {
        throw new ArchiveSerializationException($"Requires size is {expected} but buffer length is {actual}.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidAdvance()
    {
        throw new ArchiveSerializationException($"Cannot advance past the end of the buffer.");
    }

    [DoesNotReturn]
    public static void ThrowSequenceReachedEnd()
    {
        throw new ArchiveSerializationException($"Sequence reached end, reader can not provide more buffer.");
    }

    [DoesNotReturn]
    public static void ThrowWriteInvalidMemberCount(byte memberCount)
    {
        throw new ArchiveSerializationException($"MemberCount/Tag allows < 250 but try to write {memberCount}.");
    }

    [DoesNotReturn]
    public static void ThrowInsufficientBufferUnless(int length)
    {
        throw new ArchiveSerializationException($"Length header size is larger than buffer size, length: {length}.");
    }

    [DoesNotReturn]
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new ArchiveSerializationException($"{type.FullName} is not registered in this provider.");
    }

    [DoesNotReturn]
    public static void ThrowRegisterInProviderFailed(Type type, Exception innerException)
    {
        throw new ArchiveSerializationException(
            $"{type.FullName} is failed in provider at creating formatter.",
            innerException
        );
    }

    [DoesNotReturn]
    public static void ThrowNotFoundInUnionType(Type actualType, Type baseType)
    {
        throw new ArchiveSerializationException(
            $"Type {actualType.FullName} is not annotated in {baseType.FullName} MemoryPackUnion."
        );
    }

    [DoesNotReturn]
    public static void ThrowInvalidTag(ushort tag, Type baseType)
    {
        throw new ArchiveSerializationException(
            $"Data read tag: {tag} but not found in {baseType.FullName} MemoryPackUnion annotations."
        );
    }

    [DoesNotReturn]
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new ArchiveSerializationException(
            $"Serializing Type '{type}' reached depth limit, maybe detect circular reference."
        );
    }

    [DoesNotReturn]
    public static void ThrowInvalidConcurrrentCollectionOperation()
    {
        throw new ArchiveSerializationException(
            $"ConcurrentCollection is Added/Removed in serializing, however serialize concurrent collection is not thread-safe."
        );
    }

    [DoesNotReturn]
    public static void ThrowDeserializeObjectIsNull(string target)
    {
        throw new ArchiveSerializationException($"Deserialized {target} is null.");
    }

    [DoesNotReturn]
    public static void ThrowFailedEncoding(OperationStatus status)
    {
        throw new ArchiveSerializationException($"Failed in Utf8 encoding/decoding process, status: {status}.");
    }

    [DoesNotReturn]
    public static void ThrowCompressionFailed(OperationStatus status)
    {
        throw new ArchiveSerializationException(
            $"Failed in Brotli compression/decompression process, status: {status}."
        );
    }

    [DoesNotReturn]
    public static void ThrowCompressionFailed()
    {
        throw new ArchiveSerializationException($"Failed in Brotli compression/decompression process.");
    }

    [DoesNotReturn]
    public static void ThrowAlreadyDecompressed()
    {
        throw new ArchiveSerializationException(
            $"BrotliDecompressor can not invoke Decompress twice, already invoked."
        );
    }

    [DoesNotReturn]
    public static void ThrowDecompressionSizeLimitExceeded(int limit, int size)
    {
        throw new ArchiveSerializationException($"In decompress process, limit is {limit} but target size is {size}.");
    }
}
