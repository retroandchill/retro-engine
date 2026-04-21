// // @file SceneText.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.World;

namespace RetroEngine.Test.World;

public class SceneTest
{
    [Test]
    public void DisposingTheSceneManagerShouldDisposeAllScenes()
    {
        Scene scene;
        using (var sceneManager = new SceneManager())
        {
            scene = new Scene(sceneManager);
        }

        Assert.That(scene.Disposed);
    }

    [Test]
    public void DisposingTheSceneDisposesObjects()
    {
        using var sceneManager = new SceneManager();
        SceneObject obj;
        using (var scene = new Scene(sceneManager))
        {
            obj = new Sprite(scene);
        }

        Assert.That(obj.Disposed);
    }

    [Test]
    public void DisposingTheSceneManagerDisposesObjects()
    {
        SceneObject obj;
        using (var sceneManager = new SceneManager())
        {
            var scene = new Scene(sceneManager);
            obj = new Sprite(scene);
        }

        Assert.That(obj.Disposed);
    }
}
