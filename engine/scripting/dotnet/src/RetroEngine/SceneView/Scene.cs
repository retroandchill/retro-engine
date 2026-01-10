// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.SceneView;

public sealed class Scene
{
    private readonly List<Viewport> _viewports = [];
    public IReadOnlyList<Viewport> Viewports => _viewports;

    public void AddViewport(Viewport viewport)
    {
        _viewports.Add(viewport);
    }

    public void RemoveViewport(Viewport viewport)
    {
        _viewports.Remove(viewport);
    }
}
