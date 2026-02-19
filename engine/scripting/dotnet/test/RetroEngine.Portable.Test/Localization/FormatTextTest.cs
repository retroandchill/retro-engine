// // @file FormatTextEst.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Test.Localization;

public class FormatTextTest
{
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
