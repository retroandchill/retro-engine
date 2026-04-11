// // @file ErrorArchiveFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MagicArchive.Formatters;

internal sealed class ErrorArchiveFormatter(Type type, string? message = null) : IArchiveFormatter
{
    public void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in object? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        Throw();
    }

    public void Deserialize(ref ArchiveReader reader, scoped ref object? value)
    {
        Throw();
    }

    [DoesNotReturn]
    private void Throw()
    {
        if (message is not null)
        {
            ArchiveSerializationException.ThrowMessage(message);
        }
        else
        {
            ArchiveSerializationException.ThrowNotRegisteredInProvider(type);
        }
    }
}

internal sealed class ErrorArchiveFormatter<T> : ArchiveFormatter<T>
{
    private readonly Exception? _exception;
    private readonly string? _message;

    public ErrorArchiveFormatter()
    {
        _exception = null;
        _message = null;
    }

    public ErrorArchiveFormatter(Exception exception)
    {
        _exception = exception;
        _message = null;
    }

    public ErrorArchiveFormatter(string message)
    {
        _exception = null;
        _message = message;
    }

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
    {
        Throw();
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T? value)
    {
        Throw();
    }

    [DoesNotReturn]
    private void Throw()
    {
        if (_exception is not null)
        {
            ArchiveSerializationException.ThrowRegisterInProviderFailed(typeof(T), _exception);
        }
        else if (_message is not null)
        {
            ArchiveSerializationException.ThrowMessage(_message);
        }
        else
        {
            ArchiveSerializationException.ThrowNotRegisteredInProvider(typeof(T));
        }
    }
}
