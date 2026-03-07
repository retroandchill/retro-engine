// // @file NativeConfigureContext.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices.Marshalling;

namespace RetroEngine.Interop;

[NativeMarshalling(typeof(NativeConfigureContextMarshaller))]
public readonly ref struct NativeConfigureContext
{
    internal IntPtr NativeObject { get; }

    internal NativeConfigureContext(IntPtr nativeObject)
    {
        NativeObject = nativeObject;
    }
}

[CustomMarshaller(typeof(NativeConfigureContext), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToNative))]
public static class NativeConfigureContextMarshaller
{
    public static class ManagedToNative
    {
        public static IntPtr ConvertToUnmanaged(NativeConfigureContext context) => context.NativeObject;
    }
}
