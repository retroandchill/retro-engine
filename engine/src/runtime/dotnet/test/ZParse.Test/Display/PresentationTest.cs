// // @file PresentationTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZParse.Display;
using ZParse.Parsers;

namespace ZParse.Test.Display;

public class PresentationTest
{
    [Test]
    public void ProperNameIsDisplayedWhenNonGraphicalCausesFailure()
    {
        var result = Character.EqualTo('a').TryParse("\x2007");

        Assert.That(
            result.ToString(),
            Is.EqualTo("Syntax error (line 1, column 1): unexpected U+2007 figure space, expected `a`.")
        );
    }

    [Test]
    public void ProperNameIsDisplayedWhenNonGraphicalIsFailed()
    {
        var result = Character.EqualTo('\x2007').TryParse("a");
        Assert.That(
            result.ToString(),
            Is.EqualTo("Syntax error (line 1, column 1): unexpected `a`, expected U+2007 figure space.")
        );
    }
}
