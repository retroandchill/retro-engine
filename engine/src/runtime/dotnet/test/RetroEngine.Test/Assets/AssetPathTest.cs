// // @file AssetPathTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using RetroEngine.Assets;

namespace RetroEngine.Test.Assets;

public class AssetPathTest
{
    [Test]
    public void DefaultAssetPathIsInvalid()
    {
        var path = default(AssetPath);
        Assert.That(path.IsValid, Is.False);
    }

    [Test]
    public void CreateFromStringPath()
    {
        var path = new AssetPath("Assets:foo.png");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(path.IsValid, Is.True);
            Assert.That(path.PackageName.ToString(), Is.EqualTo("Assets"));
            Assert.That(path.AssetName.ToString(), Is.EqualTo("foo.png"));
        }
    }
}
