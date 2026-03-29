// // @file TextTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Test.Localization;

public class TextTest
{
    private const string TextNamespace = "Core.Tests.TextFormatTest";

    private CultureManager.CultureStateSnapshot _cultureState;

    private static Text Loctext(string key, string source) => Text.AsLocalizable(TextNamespace, key, source);

    [SetUp]
    public void Setup()
    {
        _cultureState = CultureManager.Instance.BackupCultureState();
    }

    [TearDown]
    public void TearDown()
    {
        CultureManager.Instance.RestoreCultureState(_cultureState);
    }

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

    [Test]
    public void TextFormattingOrderedArgs()
    {
        using var scope = Assert.EnterMultipleScope();
        var testText = Text.AsCultureInvariant("Format with single apostrophes quotes: '{0}'");
        Assert.That(
            Text.Format(testText, ArgText0),
            Is.EqualTo(Text.AsCultureInvariant("Format with single apostrophes quotes: 'Arg0'"))
        );
        testText = Text.AsCultureInvariant("Format with double apostrophes quotes: ''{0}''");
        Assert.That(
            Text.Format(testText, ArgText0),
            Is.EqualTo(Text.AsCultureInvariant("Format with double apostrophes quotes: ''Arg0''"))
        );
        testText = Text.AsCultureInvariant("Print with single graves: `{0}`");
        Assert.That(
            Text.Format(testText, ArgText0),
            Is.EqualTo(Text.AsCultureInvariant("Print with single graves: {0}`"))
        );
        testText = Text.AsCultureInvariant("Format with double graves: ``{0}``");
        Assert.That(
            Text.Format(testText, ArgText0),
            Is.EqualTo(Text.AsCultureInvariant("Format with double graves: `Arg0`"))
        );

        testText = Text.AsCultureInvariant("Testing `escapes` here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing `escapes` here.")));
        testText = Text.AsCultureInvariant("Testing ``escapes` here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing `escapes` here.")));
        testText = Text.AsCultureInvariant("Testing ``escapes`` here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing `escapes` here.")));

        testText = Text.AsCultureInvariant("Testing `}escapes{ here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing }escapes{ here.")));
        testText = Text.AsCultureInvariant("Testing `}escapes{ here.`");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing }escapes{ here.`")));
        testText = Text.AsCultureInvariant("Testing `}escapes{` here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing }escapes{` here.")));
        testText = Text.AsCultureInvariant("Testing }escapes`{ here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing }escapes{ here.")));
        testText = Text.AsCultureInvariant("`Testing }escapes`{ here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("`Testing }escapes{ here.")));

        testText = Text.AsCultureInvariant("Testing `{escapes} here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing {escapes} here.")));
        testText = Text.AsCultureInvariant("Testing `{escapes} here.`");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing {escapes} here.`")));
        testText = Text.AsCultureInvariant("Testing `{escapes}` here.");
        Assert.That(Text.Format(testText), Is.EqualTo(Text.AsCultureInvariant("Testing {escapes}` here.")));

        testText = Text.AsCultureInvariant("Starting text: {0} {1}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Starting text: Arg0 Arg1"))
        );
        testText = Text.AsCultureInvariant("{0} {1} - Ending Text.");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 Arg1 - Ending Text."))
        );
        testText = Text.AsCultureInvariant("Starting text: {0} {1} - Ending Text.");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Starting text: Arg0 Arg1 - Ending Text."))
        );
        testText = Text.AsCultureInvariant("{0} {1}");
        Assert.That(Text.Format(testText, ArgText0, ArgText1), Is.EqualTo(Text.AsCultureInvariant("Arg0 Arg1")));
        testText = Text.AsCultureInvariant("{1} {0}");
        Assert.That(Text.Format(testText, ArgText0, ArgText1), Is.EqualTo(Text.AsCultureInvariant("Arg1 Arg0")));
        testText = Text.AsCultureInvariant("{0}");
        Assert.That(Text.Format(testText, ArgText0), Is.EqualTo(Text.AsCultureInvariant("Arg0")));
        testText = Text.AsCultureInvariant("{0} - {1} - {2} - {3}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1, ArgText2, ArgText3),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 - Arg1 - Arg2 - Arg3"))
        );
        testText = Text.AsCultureInvariant("{0} - {0} - {0} - {1}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 - Arg0 - Arg0 - Arg1"))
        );

        testText = Text.AsCultureInvariant("Starting text: {1}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Starting text: Arg1"))
        );
        testText = Text.AsCultureInvariant("{0} - Ending Text.");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 - Ending Text."))
        );
        testText = Text.AsCultureInvariant("Starting text: {0} - Ending Text.");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Starting text: Arg0 - Ending Text."))
        );

        testText = Text.AsCultureInvariant("{0} {2}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1, ArgText2),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 Arg2"))
        );
        testText = Text.AsCultureInvariant("{1}");
        Assert.That(Text.Format(testText, ArgText0, ArgText1, ArgText2), Is.EqualTo(Text.AsCultureInvariant("Arg1")));

        testText = Text.AsCultureInvariant("Starting text: {0} {1}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Starting text: Arg0 Arg1"))
        );
        testText = Text.AsCultureInvariant("{0} {1} - Ending Text.");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 Arg1 - Ending Text."))
        );
        testText = Text.AsCultureInvariant("Starting text: {0} {1} - Ending Text.");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Starting text: Arg0 Arg1 - Ending Text."))
        );
        testText = Text.AsCultureInvariant("{0} {1}");
        Assert.That(Text.Format(testText, ArgText0, ArgText1), Is.EqualTo(Text.AsCultureInvariant("Arg0 Arg1")));
        testText = Text.AsCultureInvariant("{1} {0}");
        Assert.That(Text.Format(testText, ArgText0, ArgText1), Is.EqualTo(Text.AsCultureInvariant("Arg1 Arg0")));
        testText = Text.AsCultureInvariant("{0}");
        Assert.That(Text.Format(testText, ArgText0), Is.EqualTo(Text.AsCultureInvariant("Arg0")));
        testText = Text.AsCultureInvariant("{0} - {1} - {2} - {3}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1, ArgText2, ArgText3),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 - Arg1 - Arg2 - Arg3"))
        );
        testText = Text.AsCultureInvariant("{0} - {0} - {0} - {1}");
        Assert.That(
            Text.Format(testText, ArgText0, ArgText1),
            Is.EqualTo(Text.AsCultureInvariant("Arg0 - Arg0 - Arg0 - Arg1"))
        );
    }

    [Test]
    public void TextFormattingFromNamedArguments()
    {
        var arguments = new Dictionary<string, FormatArg>
        {
            ["Age"] = Text.AsCultureInvariant("23"),
            ["Height"] = Text.AsCultureInvariant("68"),
            ["Gender"] = Text.AsCultureInvariant("male"),
            ["Name"] = Text.AsCultureInvariant("Saul"),
        };

        using var scope = Assert.EnterMultipleScope();
        var testText = Text.AsCultureInvariant("My name is {Name}.");
        Assert.That(Text.Format(testText, arguments), Is.EqualTo(Text.AsCultureInvariant("My name is Saul.")));
        testText = Text.AsCultureInvariant("My age is {Age}.");
        Assert.That(Text.Format(testText, arguments), Is.EqualTo(Text.AsCultureInvariant("My age is 23.")));
        testText = Text.AsCultureInvariant("My gender is {Gender}.");
        Assert.That(Text.Format(testText, arguments), Is.EqualTo(Text.AsCultureInvariant("My gender is male.")));
        testText = Text.AsCultureInvariant("My height is {Height}.");
        Assert.That(Text.Format(testText, arguments), Is.EqualTo(Text.AsCultureInvariant("My height is 68.")));

        // Using arguments out of order is okay.
        testText = Text.AsCultureInvariant("My name is {Name}. My age is {Age}. My gender is {Gender}.");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("My name is Saul. My age is 23. My gender is male."))
        );
        testText = Text.AsCultureInvariant("My age is {Age}. My gender is {Gender}. My name is {Name}.");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("My age is 23. My gender is male. My name is Saul."))
        );
        testText = Text.AsCultureInvariant("My gender is {Gender}. My name is {Name}. My age is {Age}.");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("My gender is male. My name is Saul. My age is 23."))
        );
        testText = Text.AsCultureInvariant("My gender is {Gender}. My age is {Age}. My name is {Name}.");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("My gender is male. My age is 23. My name is Saul."))
        );
        testText = Text.AsCultureInvariant("My age is {Age}. My name is {Name}. My gender is {Gender}.");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("My age is 23. My name is Saul. My gender is male."))
        );
        testText = Text.AsCultureInvariant("My name is {Name}. My gender is {Gender}. My age is {Age}.");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("My name is Saul. My gender is male. My age is 23."))
        );

        // Reusing arguments is okay.
        testText = Text.AsCultureInvariant("If my age is {Age}, I have been alive for {Age} year(s).");
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(Text.AsCultureInvariant("If my age is 23, I have been alive for 23 year(s)."))
        );

        // Not providing an argument leaves the parameter as text.
        testText = Text.AsCultureInvariant(
            "What... is the air-speed velocity of an unladen swallow? {AirSpeedOfAnUnladenSwallow}."
        );
        Assert.That(
            Text.Format(testText, arguments),
            Is.EqualTo(
                Text.AsCultureInvariant(
                    "What... is the air-speed velocity of an unladen swallow? {AirSpeedOfAnUnladenSwallow}."
                )
            )
        );
    }
}
