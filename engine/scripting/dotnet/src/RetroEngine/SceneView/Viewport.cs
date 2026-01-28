// @file 2026.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.SceneView;

public sealed partial class Viewport(Vector2F viewportSize) : SceneObject(NativeCreate(viewportSize)), IViewSync
{
    public Vector2F Size
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            MarkAsDirty();
        }
    } = viewportSize;

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_viewport_create")]
    private static partial IntPtr NativeCreate(Vector2F viewportSize);
}
