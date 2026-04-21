// // @file InteropError.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Utilities;

namespace RetroEngine.Interop;

public enum InteropErrorCode
{
    None,
    Unknown,
    IoError,
    ResourceError,
    InvalidState,
    UnsupportedOperation,
    NotImplemented,
    PlatformError,
    GraphicsError,
    InvalidArgument,
    OutOfRange,
    BadAlloc,
}

[InheritConstructors]
public sealed partial class NativeInteropException : Exception;

[NativeMarshalling(typeof(InteropErrorMarshaller))]
public readonly record struct InteropError(InteropErrorCode ErrorCode, string? NativeExceptionType, string ErrorMessage)
{
    private string FullErrorMessage =>
        string.IsNullOrEmpty(NativeExceptionType) ? ErrorMessage : $"{NativeExceptionType}: {ErrorMessage}";

    public void ThrowIfError()
    {
        var exception = ToException();
        if (exception is not null)
            throw exception;
    }

    private Exception? ToException()
    {
        return ErrorCode switch
        {
            InteropErrorCode.None => null,
            InteropErrorCode.Unknown => new NativeInteropException(FullErrorMessage),
            InteropErrorCode.IoError => new IOException(FullErrorMessage),
            InteropErrorCode.ResourceError => new ResourceException(FullErrorMessage),
            InteropErrorCode.InvalidState => new InvalidStateException(FullErrorMessage),
            InteropErrorCode.UnsupportedOperation => new NotSupportedException(FullErrorMessage),
            InteropErrorCode.NotImplemented => new NotImplementedException(FullErrorMessage),
            InteropErrorCode.PlatformError => new PlatformNotSupportedException(FullErrorMessage),
            InteropErrorCode.GraphicsError => new GraphicsException(FullErrorMessage),
            InteropErrorCode.InvalidArgument => new ArgumentException(FullErrorMessage),
            InteropErrorCode.OutOfRange => new ArgumentOutOfRangeException(null, FullErrorMessage),
            InteropErrorCode.BadAlloc => new OutOfMemoryException(FullErrorMessage),
            _ => new InvalidOperationException("Unknown error code."),
        };
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct NativeInteropError
{
    public InteropErrorCode ErrorCode;
    public byte* NativeExceptionType;
    public byte* ErrorMessage;
}

[CustomMarshaller(typeof(InteropError), MarshalMode.ManagedToUnmanagedOut, typeof(NativeToManaged))]
public static class InteropErrorMarshaller
{
    public static unsafe class NativeToManaged
    {
        public static InteropError ConvertToManaged(NativeInteropError bytes)
        {
            return new InteropError(
                bytes.ErrorCode,
                Utf8StringMarshaller.ConvertToManaged(bytes.NativeExceptionType),
                Utf8StringMarshaller.ConvertToManaged(bytes.ErrorMessage) ?? "No error occurred"
            );
        }
    }
}
