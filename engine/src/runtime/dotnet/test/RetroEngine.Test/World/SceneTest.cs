// @file SceneText.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.World;

namespace RetroEngine.Test.World;

public class SceneTest
{
    [Test]
    public void DisposingTheSceneManagerShouldDisposeAllScenes()
    {
        using var sceneManager = new SceneManager();
        var contextLifetime = new SceneContextLifetime(sceneManager);
        Scene scene;
        using (contextLifetime.CreateLifetimeScope())
        {
            scene = new Scene();
        }

        Assert.That(scene.Disposed);
    }

    [Test]
    public void DisposingTheSceneDisposesObjects()
    {
        using var sceneManager = new SceneManager();
        var contextLifetime = new SceneContextLifetime(sceneManager);
        using var scope = contextLifetime.CreateLifetimeScope();
        SceneObject obj;
        using (var scene = new Scene())
        {
            obj = new Sprite(scene);
        }

        Assert.That(obj.Disposed);
    }

    [Test]
    public void DisposingTheSceneManagerDisposesObjects()
    {
        using var sceneManager = new SceneManager();
        var contextLifetime = new SceneContextLifetime(sceneManager);
        SceneObject obj;
        using (contextLifetime.CreateLifetimeScope())
        {
            var scene = new Scene();
            obj = new Sprite(scene);
        }

        Assert.That(obj.Disposed);
    }
}
