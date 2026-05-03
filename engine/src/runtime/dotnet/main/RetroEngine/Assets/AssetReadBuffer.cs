// @file AssetReadBufferProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets;

internal static class AssetReadBufferPool
{
    private static readonly ConcurrentQueue<AssetReadBuffer> Buffers = new();

    public static AssetReadBuffer Rent()
    {
        if (Buffers.TryDequeue(out var buffer))
            return buffer;

        var newBuffer = new AssetReadBuffer();
        return newBuffer;
    }

    public static void Return(AssetReadBuffer buffer)
    {
        buffer.Reset();
        Buffers.Enqueue(buffer);
    }
}

internal sealed partial class AssetReadBuffer : IDisposable
{
    private byte[]? _buffer;
    private int _offset;

    public ReadOnlySpan<byte> Span => _buffer is not null ? _buffer.AsSpan(_offset) : default;

    public ReadOnlyMemory<byte> Memory => _buffer?.AsMemory(0, _offset) ?? default;

    [CreateSyncVersion]
    public async ValueTask ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (_buffer is null)
        {
            _buffer = stream.CanSeek
                ? ArrayPool<byte>.Shared.Rent((int)stream.Length)
                : ArrayPool<byte>.Shared.Rent(65536);
            _offset = 0;
        }

        do
        {
            if (_offset == _buffer.Length)
            {
                var newSize = unchecked(2 * _buffer.Length);
                if ((uint)newSize > int.MaxValue)
                    newSize = int.MaxValue;

                var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
                _buffer.AsSpan().CopyTo(newBuffer);
                ArrayPool<byte>.Shared.Return(_buffer);
            }

            var read = await stream.ReadAsync(_buffer.AsMemory(_offset), cancellationToken).ConfigureAwait(false);

            _offset += read;

            if (read == 0)
                break;
        } while (true);
    }

    public void Reset()
    {
        if (_buffer is not null)
            ArrayPool<byte>.Shared.Return(_buffer);

        _buffer = null;
    }

    public void Dispose()
    {
        AssetReadBufferPool.Return(this);
    }
}
