// // @file FormatTextEst.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Test.Localization;

public class FormatTextTest
{
    [Test]
    public unsafe void TestIcuInterop()
    {
        var enBytes = Encoding.UTF8.GetBytes("en_US");
        Span<char> targetBuffer = stackalloc char[32];
        var resultString = IcuInterop.GetDisplayName(enBytes, enBytes, targetBuffer);
        var targetString = new string(resultString.ToArray());
        Assert.That(targetString, Is.EqualTo("en_US"));
    }

    [Test]
    public void CanDenotePluralForms()
    {
        var format = new TextFormat(
            "There {NumCats}|plural(one=is,other=are) {NumCats} {NumCats}|plural(one=cat,other=cats)"
        );

        Assert.That(format.IsValid);

        var formatted = TextFormatter.FormatStr(
            format,
            new Dictionary<string, FormatArg> { ["NumCats"] = 1 },
            false,
            false
        );
        Assert.That(formatted, Is.EqualTo("There is 1 cat"));

        formatted = TextFormatter.FormatStr(
            format,
            new Dictionary<string, FormatArg> { ["NumCats"] = 2 },
            false,
            false
        );
        Assert.That(formatted, Is.EqualTo("There are 2 cats"));

        formatted = TextFormatter.FormatStr(
            format,
            new Dictionary<string, FormatArg> { ["NumCats"] = 0 },
            false,
            false
        );
        Assert.That(formatted, Is.EqualTo("There are 0 cats"));
    }
}
