// // @file AssetPathCursorTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;

namespace RetroEngine.Test.Assets;

public class AssetPathCursorTest
{
    [Test]
    public void AdvancingWorksAsExpected()
    {
        const string path = "foo/bar/baz";

        var cursor = new AssetPathCursor(path, '/');
        using (Assert.EnterMultipleScope())
        {
            Assert.That(cursor.FullPath.ToString(), Is.EqualTo(path));
            Assert.That(cursor.CurrentPath.ToString(), Is.EqualTo("foo"));
            Assert.That(cursor.RemainingPath.ToString(), Is.EqualTo("bar/baz"));
        }

        Assert.That(cursor.TryGetNextChild(out var child), Is.True);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(child.FullPath.ToString(), Is.EqualTo(path));
            Assert.That(child.CurrentPath.ToString(), Is.EqualTo("foo/bar"));
            Assert.That(child.RemainingPath.ToString(), Is.EqualTo("baz"));
        }

        Assert.That(child.TryGetNextChild(out var secondChild), Is.True);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(secondChild.FullPath.ToString(), Is.EqualTo(path));
            Assert.That(secondChild.CurrentPath.ToString(), Is.EqualTo("foo/bar/baz"));
            Assert.That(secondChild.RemainingPath.ToString(), Is.EqualTo(""));
        }

        Assert.That(secondChild.TryGetNextChild(out var thirdChild), Is.False);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(thirdChild.FullPath.ToString(), Is.EqualTo(path));
            Assert.That(thirdChild.CurrentPath.ToString(), Is.EqualTo("foo/bar/baz"));
            Assert.That(thirdChild.RemainingPath.ToString(), Is.EqualTo(""));
        }
    }
}
