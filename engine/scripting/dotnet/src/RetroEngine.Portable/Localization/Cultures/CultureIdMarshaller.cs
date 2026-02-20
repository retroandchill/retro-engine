// // @file CultureIdMarshaller.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace RetroEngine.Portable.Localization.Cultures;

[CustomMarshaller(typeof(CultureId), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToNative))]
internal static class CultureIdMarshaller
{
    public static unsafe class ManagedToNative
    {
        public static byte* ConvertToUnmanaged(ref CultureId cultureId)
        {
            return (byte*)GCHandle.Alloc(cultureId.Utf8Bytes, GCHandleType.Pinned).AddrOfPinnedObject();
        }

        public static ref readonly byte GetPinnableReference(CultureId cultureId)
        {
            return ref cultureId.Utf8Bytes[0];
        }

        public static void Free(byte* bytes)
        {
            GCHandle.FromIntPtr((IntPtr)bytes).Free();
        }
    }
}
