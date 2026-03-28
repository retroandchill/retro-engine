// // @file FriendlyTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;
using ZParse.Display;

namespace ZParse.Test.Display;

public class FriendlyTest
{
    [Test]
    public void FriendlyListsPreserveOrderButRemoveDuplicates()
    {
        var builder = new ValueStringBuilder();

        try
        {
            builder.AppendFriendlyList(["one", "two", "two", "one", "three"]);
            const string expected = "one, two, or three";

            Assert.That(builder.ToString(), Is.EqualTo(expected));
        }
        finally
        {
            builder.Dispose();
        }
    }
}
