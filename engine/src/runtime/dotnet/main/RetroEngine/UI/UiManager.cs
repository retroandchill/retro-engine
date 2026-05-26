// @file UiManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Tickables;

namespace RetroEngine.UI;

[RegisterSingleton]
public sealed class UiManager(TickManager tickManager) : IUiManager
{
    private readonly HashSet<IUiRoot> _roots = new(ReferenceEqualityComparer.Instance);

    public IUiRoot CreateNewRoot()
    {
        var root = new UiRoot(this, tickManager);
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
