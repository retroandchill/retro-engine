// // @file UnownedCharMarshaller.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices.Marshalling;

namespace RetroEngine.Interop;

[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(NativeToManaged))]
public static class UnownedCharMarshaller
{
    public static unsafe class NativeToManaged
    {
        public static string? ConvertToManaged(byte* bytes)
        {
            return Utf8StringMarshaller.ConvertToManaged(bytes);
        }
    }
}
