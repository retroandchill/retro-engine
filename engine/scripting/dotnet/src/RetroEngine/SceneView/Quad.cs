// // @file $FILE$
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Interop;
using RetroEngine.Strings;

namespace RetroEngine.SceneView;

public sealed class Quad(Viewport viewport) : SceneObject(Type, viewport)
{
    private static readonly Name Type = new("Quad");

    public Vector2F Size
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            QuadExporter.SetQuadSize(Id, value);
        }
    }

    public Color Color
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            QuadExporter.SetQuadColor(Id, field);
        }
    }
}
