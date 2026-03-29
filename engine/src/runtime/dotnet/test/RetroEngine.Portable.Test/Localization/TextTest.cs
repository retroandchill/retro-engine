// // @file TextTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;

namespace RetroEngine.Portable.Test.Localization;

public class TextTest
{
    private const string TextNamespace = "Core.Tests.TextFormatTest";

    private static Text Loctext(string key, string source) => Text.AsLocalizable(TextNamespace, key, source);

    private static readonly Text ArgText0 = Text.AsCultureInvariant("Arg0");
    private static readonly Text ArgText1 = Text.AsCultureInvariant("Arg1");
    private static readonly Text ArgText2 = Text.AsCultureInvariant("Arg2");
    private static readonly Text ArgText3 = Text.AsCultureInvariant("Arg3");

    [Test]
    public void IdenticalToComparison()
    {
        const int testNumber1 = 10;
        const int testNumber2 = 20;
        DateTimeOffset testDate = new DateTime(1991, 6, 21, 9, 30, 0, DateTimeKind.Utc);
        var testIdenticalStr1 = Loctext("TestIdenticalStr1", "Str1");
        var testIdenticalStr2 = Loctext("TestIdenticalStr2", "Str2");

        using var scope = Assert.EnterMultipleScope();
        AssertEqual(testIdenticalStr1, testIdenticalStr1, TextIdenticalModeFlags.None, true);
        AssertEqual(testIdenticalStr1, testIdenticalStr2, TextIdenticalModeFlags.None, false);
        AssertEqual(
            testIdenticalStr1,
            testIdenticalStr1,
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            true
        );
        AssertEqual(
            testIdenticalStr1,
            testIdenticalStr2,
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );

        AssertEqual(
            Text.AsCultureInvariant("Wooble"),
            Text.AsCultureInvariant("Wooble"),
            TextIdenticalModeFlags.None,
            false
        );
        AssertEqual(new Text("Wooble"), new Text("Wooble"), TextIdenticalModeFlags.None, false);
        AssertEqual(
            Text.AsCultureInvariant("Wooble"),
            Text.AsCultureInvariant("Wooble"),
            TextIdenticalModeFlags.LexicalCompareInvariants,
            true
        );
        AssertEqual(new Text("Wooble"), new Text("Wooble"), TextIdenticalModeFlags.LexicalCompareInvariants, true);
        AssertEqual(
            Text.AsCultureInvariant("Wooble"),
            Text.AsCultureInvariant("Wooble2"),
            TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );
        AssertEqual(new Text("Wooble"), new Text("Wooble2"), TextIdenticalModeFlags.LexicalCompareInvariants, false);

        AssertEqual(
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            TextIdenticalModeFlags.None,
            false
        );
        AssertEqual(
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            true
        );
        AssertEqual(
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText1),
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );
        AssertEqual(
            Text.Format(Loctext("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(Loctext("TestIdenticalPattern2", "This takes an arg {0}!"), ArgText0),
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );

        AssertEqual(Text.AsDate(testDate), Text.AsDate(testDate), TextIdenticalModeFlags.None, false);
        AssertEqual(Text.AsDate(testDate), Text.AsDate(testDate), TextIdenticalModeFlags.DeepCompare, true);
        AssertEqual(Text.AsTime(testDate), Text.AsTime(testDate), TextIdenticalModeFlags.None, false);
        AssertEqual(Text.AsTime(testDate), Text.AsTime(testDate), TextIdenticalModeFlags.DeepCompare, true);
        AssertEqual(Text.AsDateTime(testDate), Text.AsDateTime(testDate), TextIdenticalModeFlags.None, false);
        AssertEqual(Text.AsDateTime(testDate), Text.AsDateTime(testDate), TextIdenticalModeFlags.DeepCompare, true);

        AssertEqual(Text.AsNumber(testNumber1), Text.AsNumber(testNumber1), TextIdenticalModeFlags.None, false);
        AssertEqual(Text.AsNumber(testNumber1), Text.AsNumber(testNumber1), TextIdenticalModeFlags.DeepCompare, true);
        AssertEqual(Text.AsNumber(testNumber1), Text.AsNumber(testNumber2), TextIdenticalModeFlags.None, false);
        AssertEqual(Text.AsNumber(testNumber1), Text.AsNumber(testNumber2), TextIdenticalModeFlags.DeepCompare, false);

        AssertEqual(testIdenticalStr1.ToUpper(), testIdenticalStr1.ToUpper(), TextIdenticalModeFlags.None, false);
        AssertEqual(testIdenticalStr1.ToUpper(), testIdenticalStr1.ToUpper(), TextIdenticalModeFlags.DeepCompare, true);
        AssertEqual(testIdenticalStr1.ToUpper(), testIdenticalStr1.ToLower(), TextIdenticalModeFlags.None, false);
        AssertEqual(
            testIdenticalStr1.ToUpper(),
            testIdenticalStr1.ToLower(),
            TextIdenticalModeFlags.DeepCompare,
            false
        );
        AssertEqual(testIdenticalStr1.ToUpper(), testIdenticalStr2.ToUpper(), TextIdenticalModeFlags.None, false);
        AssertEqual(
            testIdenticalStr1.ToUpper(),
            testIdenticalStr2.ToUpper(),
            TextIdenticalModeFlags.DeepCompare,
            false
        );

        return;

        static void AssertEqual(Text a, Text b, TextIdenticalModeFlags flags, bool expected)
        {
            Assert.That(a.IdenticalTo(b, flags), expected ? Is.True : Is.False);
        }
    }
}
