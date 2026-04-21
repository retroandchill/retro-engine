// // @file TickManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ZLinq;

namespace RetroEngine.Tickables;

[RegisterSingleton]
public sealed class TickManager
{
    private readonly HashSet<ITickable> _tickables = new(ReferenceEqualityComparer.Default);

    public void RegisterTickable(ITickable tickable)
    {
        _tickables.Add(tickable);
    }

    public void UnregisterTickable(ITickable tickable)
    {
        _tickables.Remove(tickable);
    }

    public void Tick(float deltaTime)
    {
        foreach (var tickable in _tickables.AsValueEnumerable().Where(t => t.TickEnabled))
        {
            tickable.Tick(deltaTime);
        }
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<ITickable>
    {
        public static readonly ReferenceEqualityComparer Default = new();

        public bool Equals(ITickable? x, ITickable? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(ITickable obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
