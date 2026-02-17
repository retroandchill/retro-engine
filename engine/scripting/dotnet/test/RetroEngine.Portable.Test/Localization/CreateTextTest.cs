// // @file TextFromNumberTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;

namespace RetroEngine.Portable.Test.Localization;

public class CreateTextTest
{
    [Test]
    public void CreateTextFromNumbers()
    {
        var text1 = Text.AsNumber(123);
        Assert.That(text1.ToString(), Is.EqualTo("123"));

        var text2 = Text.AsNumber(123.456);
        Assert.That(text2.ToString(), Is.EqualTo("123.456"));
    }
}
