// // @file SynchronizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Core.State;

public static class NativeSynchronizationManager
{
    private static HashSet<INativeSynchronizable> _pendingSync = [];
    private static HashSet<INativeSynchronizable> _activeSync = [];

    public static void AddToSyncList(this INativeSynchronizable syncable)
    {
        _pendingSync.Add(syncable);
    }

    public static void Sync()
    {
        (_pendingSync, _activeSync) = (_activeSync, _pendingSync);

        foreach (var syncable in _activeSync)
        {
            syncable.SyncToNative();
        }
        _activeSync.Clear();
    }
}
