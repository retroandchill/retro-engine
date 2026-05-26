// @file UiManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Tickables;
using RetroEngine.World;

namespace RetroEngine.UI;

[RegisterSingleton]
public sealed class UiManager(TickManager tickManager, SceneManager sceneManager, ViewportManager viewportManager)
    : IUiManager
{
    private readonly HashSet<IUiRoot> _roots = new(ReferenceEqualityComparer.Instance);

    public IUiRoot CreateNewRoot()
    {
        var root = new UiRoot(this, tickManager, sceneManager, viewportManager);
        _roots.Add(root);
        return root;
    }

    public void DestroyRoot(IUiRoot root)
    {
        if (!_roots.Remove(root))
            return;

        if (!root.Disposed)
            return;

        root.Dispose();
    }
}
