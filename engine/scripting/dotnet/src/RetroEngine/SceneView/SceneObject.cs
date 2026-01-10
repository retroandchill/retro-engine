// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Math;

namespace RetroEngine.SceneView;

public abstract class SceneObject
{
    public Transform Transform { get; set; }
    public Viewport Viewport { get; internal set; }
}
