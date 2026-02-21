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
    public void CanDenotePluralForms()
    {
        var format = new TextFormat(
            "There {NumCats}|plural(one=is,other=are) {NumCats} {NumCats}|plural(one=cat,other=cats)"
        );

        Assert.That(format.IsValid);

        var formatted = Text.Format(format, new Dictionary<string, FormatArg> { ["NumCats"] = 1 });
        Assert.That(formatted.ToString(), Is.EqualTo("There is 1 cat"));

        formatted = Text.Format(format, new Dictionary<string, FormatArg> { ["NumCats"] = 2 });
        Assert.That(formatted.ToString(), Is.EqualTo("There are 2 cats"));

        formatted = Text.Format(format, new Dictionary<string, FormatArg> { ["NumCats"] = 0 });
        Assert.That(formatted.ToString(), Is.EqualTo("There are 0 cats"));
    }
}
