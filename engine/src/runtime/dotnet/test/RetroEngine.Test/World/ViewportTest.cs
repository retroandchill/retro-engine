// // @file ViewportTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.World;

namespace RetroEngine.Test.World;

public class ViewportTest
{
    [Test]
    public void DisposingTheViewportManagerShouldDisposeAllViewports()
    {
        Viewport viewport;
        using (var viewportManager = new ViewportManager())
        {
            viewport = new Viewport(viewportManager);
        }

        Assert.That(viewport.Disposed);
    }
}
