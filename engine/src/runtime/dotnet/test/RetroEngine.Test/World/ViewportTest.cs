// @file ViewportTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Events;
using RetroEngine.World;

namespace RetroEngine.Test.World;

public class ViewportTest
{
    [Test]
    public void DisposingTheViewportManagerShouldDisposeAllViewports()
    {
        using var eventManager = new EventManager();
        using var viewportManager = new ViewportManager(eventManager);
        var contextLifetime = new ViewportContextLifetime(viewportManager);
        Viewport viewport;
        using (contextLifetime.CreateLifetimeScope())
        {
            viewport = new Viewport();
        }

        Assert.That(viewport.Disposed);
    }
}
