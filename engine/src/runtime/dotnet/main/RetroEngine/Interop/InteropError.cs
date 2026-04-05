// // @file InteropError.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Utilities;
using RetroEngine.Utils;

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
        switch (ErrorCode)
        {
            case InteropErrorCode.None:
                break;
            case InteropErrorCode.Unknown:
                throw new NativeInteropException(FullErrorMessage);
            case InteropErrorCode.IoError:
                throw new IOException(FullErrorMessage);
            case InteropErrorCode.ResourceError:
                throw new ResourceException(FullErrorMessage);
            case InteropErrorCode.InvalidState:
                throw new InvalidStateException(FullErrorMessage);
            case InteropErrorCode.UnsupportedOperation:
                throw new NotSupportedException(FullErrorMessage);
            case InteropErrorCode.NotImplemented:
                throw new NotImplementedException(FullErrorMessage);
            case InteropErrorCode.PlatformError:
                throw new PlatformNotSupportedException(FullErrorMessage);
            case InteropErrorCode.GraphicsError:
                throw new GraphicsException(FullErrorMessage);
            case InteropErrorCode.InvalidArgument:
                throw new ArgumentException(FullErrorMessage);
            case InteropErrorCode.OutOfRange:
                throw new ArgumentOutOfRangeException(null, FullErrorMessage);
            case InteropErrorCode.BadAlloc:
                throw new OutOfMemoryException(FullErrorMessage);
            default:
                throw new InvalidOperationException("Unknown error code.");
        }
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
