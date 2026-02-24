// // @file ITickable.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

public interface ITickable
{
    bool TickEnabled { get; }

    void Tick(float deltaTime);
}
