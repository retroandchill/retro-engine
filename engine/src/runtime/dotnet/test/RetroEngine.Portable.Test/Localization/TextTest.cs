// // @file TextTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using ZLinq;

namespace RetroEngine.Portable.Test.Localization;

using RoundingModeTuple = (
    string HalfToEven,
    string HalfFromZero,
    string HalfToZero,
    string FromZero,
    string ToZero,
    string ToNegativeInfinity,
    string ToPositiveInfinity
);

public class TextTest
{
    private const string TextNamespace = "Core.Tests.TextFormatTest";

    private CultureManager.CultureStateSnapshot _cultureState;

    private static Culture? GetCulture(string culture)
    {
        return string.IsNullOrEmpty(culture) ? null : CultureManager.Instance.GetCulture(culture);
    }

    private static Text NsLocText(string ns, string key, string source) => Text.AsLocalizable(ns, key, source);

    private static Text LocText(string key, string source) => Text.AsLocalizable(TextNamespace, key, source);

    private static Text LocTable(string key, string source) => Text.AsLocalizable(TextNamespace, key, source);

    private static Text InvText(string str) => Text.AsCultureInvariant(str);

    private static Text LocGenNumber(FormatNumericArg arg, string culture) =>
        Text.AsNumber(arg, null, GetCulture(culture));

    private static Text LocGenNumberGrouped(FormatNumericArg arg, string culture) =>
        Text.AsNumber(arg, NumberFormattingOptions.DefaultWithGrouping, GetCulture(culture));

    private static Text LocGenNumberUngrouped(FormatNumericArg arg, string culture) =>
        Text.AsNumber(arg, NumberFormattingOptions.DefaultWithoutGrouping, GetCulture(culture));

    private static Text LocGenNumberCustom(FormatNumericArg arg, NumberFormattingOptions options, string culture) =>
        Text.AsNumber(arg, options, GetCulture(culture));

    private static Text LocGenPercent(FormatNumericArg arg, string culture) =>
        Text.AsPercent(arg, null, GetCulture(culture));

    private static Text LocGenPercentGrouped(FormatNumericArg arg, string culture) =>
        Text.AsPercent(arg, NumberFormattingOptions.DefaultWithGrouping, GetCulture(culture));

    private static Text LocGenPercentUngrouped(FormatNumericArg arg, string culture) =>
        Text.AsPercent(arg, NumberFormattingOptions.DefaultWithoutGrouping, GetCulture(culture));

    private static Text LocGenPercentCustom(FormatNumericArg arg, NumberFormattingOptions options, string culture) =>
        Text.AsPercent(arg, options, GetCulture(culture));

    private static Text LocGenCurrency(FormatNumericArg num, string currency, string culture) =>
        Text.AsCurrency(num, currency, null, GetCulture(culture));

    private static Text LocGenDateUtc(long unixTime, DateTimeFormatStyle dateStyle, string timeZone, string culture) =>
        Text.AsDate(DateTimeOffset.FromUnixTimeMilliseconds(unixTime), dateStyle, timeZone, GetCulture(culture));

    private static Text LocGenDateLocal(long unixTime, DateTimeFormatStyle dateStyle, string culture) =>
        Text.AsDate(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            dateStyle,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    private static Text LocGenTimeUtc(long unixTime, DateTimeFormatStyle timeStyle, string timeZone, string culture) =>
        Text.AsTime(DateTimeOffset.FromUnixTimeMilliseconds(unixTime), timeStyle, timeZone, GetCulture(culture));

    private static Text LocGenTimeLocal(long unixTime, DateTimeFormatStyle timeStyle, string culture) =>
        Text.AsTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            timeStyle,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    private static Text LocGenDateTimeUtc(
        long unixTime,
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string timeZone,
        string culture
    ) =>
        Text.AsDateTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            dateStyle,
            timeStyle,
            timeZone,
            GetCulture(culture)
        );

    private static Text LocGenDateTimeLocal(
        long unixTime,
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string culture
    ) =>
        Text.AsDateTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            dateStyle,
            timeStyle,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    private static Text LocGenDateTimeCustomUtc(long unixTime, string pattern, string timeZone, string culture) =>
        Text.AsDateTime(DateTimeOffset.FromUnixTimeMilliseconds(unixTime), pattern, timeZone, GetCulture(culture));

    private static Text LocGenDateTimeCustomLocal(long unixTime, string pattern, string culture) =>
        Text.AsDateTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            pattern,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    private static Text LocGenToUpper(Text text) => text.ToUpper();

    private static Text LocGenToLower(Text text) => text.ToLower();

    private static Text LocGenFormatOrdered(Text pattern, params ReadOnlySpan<FormatArg> args) =>
        Text.Format(pattern, args);

    private static Text LocGenFormatNamed(Text pattern, params ReadOnlySpan<(string Key, FormatArg Arg)> args) =>
        Text.Format(pattern, args.AsValueEnumerable().ToDictionary(x => x.Key, x => x.Arg));

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
        var testIdenticalStr1 = LocText("TestIdenticalStr1", "Str1");
        var testIdenticalStr2 = LocText("TestIdenticalStr2", "Str2");

        using var scope = Assert.EnterMultipleScope();
        TestIdentical(testIdenticalStr1, testIdenticalStr1, TextIdenticalModeFlags.None, true);
        TestIdentical(testIdenticalStr1, testIdenticalStr2, TextIdenticalModeFlags.None, false);
        TestIdentical(
            testIdenticalStr1,
            testIdenticalStr1,
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            true
        );
        TestIdentical(
            testIdenticalStr1,
            testIdenticalStr2,
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );

        TestIdentical(
            Text.AsCultureInvariant("Wooble"),
            Text.AsCultureInvariant("Wooble"),
            TextIdenticalModeFlags.None,
            false
        );
        TestIdentical(new Text("Wooble"), new Text("Wooble"), TextIdenticalModeFlags.None, false);
        TestIdentical(
            Text.AsCultureInvariant("Wooble"),
            Text.AsCultureInvariant("Wooble"),
            TextIdenticalModeFlags.LexicalCompareInvariants,
            true
        );
        TestIdentical(new Text("Wooble"), new Text("Wooble"), TextIdenticalModeFlags.LexicalCompareInvariants, true);
        TestIdentical(
            Text.AsCultureInvariant("Wooble"),
            Text.AsCultureInvariant("Wooble2"),
            TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );
        TestIdentical(new Text("Wooble"), new Text("Wooble2"), TextIdenticalModeFlags.LexicalCompareInvariants, false);

        TestIdentical(
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            TextIdenticalModeFlags.None,
            false
        );
        TestIdentical(
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            true
        );
        TestIdentical(
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText1),
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );
        TestIdentical(
            Text.Format(LocText("TestIdenticalPattern", "This takes an arg {0}"), ArgText0),
            Text.Format(LocText("TestIdenticalPattern2", "This takes an arg {0}!"), ArgText0),
            TextIdenticalModeFlags.DeepCompare | TextIdenticalModeFlags.LexicalCompareInvariants,
            false
        );

        TestIdentical(Text.AsDate(testDate), Text.AsDate(testDate), TextIdenticalModeFlags.None, false);
        TestIdentical(Text.AsDate(testDate), Text.AsDate(testDate), TextIdenticalModeFlags.DeepCompare, true);
        TestIdentical(Text.AsTime(testDate), Text.AsTime(testDate), TextIdenticalModeFlags.None, false);
        TestIdentical(Text.AsTime(testDate), Text.AsTime(testDate), TextIdenticalModeFlags.DeepCompare, true);
        TestIdentical(Text.AsDateTime(testDate), Text.AsDateTime(testDate), TextIdenticalModeFlags.None, false);
        TestIdentical(Text.AsDateTime(testDate), Text.AsDateTime(testDate), TextIdenticalModeFlags.DeepCompare, true);

        TestIdentical(Text.AsNumber(testNumber1), Text.AsNumber(testNumber1), TextIdenticalModeFlags.None, false);
        TestIdentical(Text.AsNumber(testNumber1), Text.AsNumber(testNumber1), TextIdenticalModeFlags.DeepCompare, true);
        TestIdentical(Text.AsNumber(testNumber1), Text.AsNumber(testNumber2), TextIdenticalModeFlags.None, false);
        TestIdentical(
            Text.AsNumber(testNumber1),
            Text.AsNumber(testNumber2),
            TextIdenticalModeFlags.DeepCompare,
            false
        );

        TestIdentical(testIdenticalStr1.ToUpper(), testIdenticalStr1.ToUpper(), TextIdenticalModeFlags.None, false);
        TestIdentical(
            testIdenticalStr1.ToUpper(),
            testIdenticalStr1.ToUpper(),
            TextIdenticalModeFlags.DeepCompare,
            true
        );
        TestIdentical(testIdenticalStr1.ToUpper(), testIdenticalStr1.ToLower(), TextIdenticalModeFlags.None, false);
        TestIdentical(
            testIdenticalStr1.ToUpper(),
            testIdenticalStr1.ToLower(),
            TextIdenticalModeFlags.DeepCompare,
            false
        );
        TestIdentical(testIdenticalStr1.ToUpper(), testIdenticalStr2.ToUpper(), TextIdenticalModeFlags.None, false);
        TestIdentical(
            testIdenticalStr1.ToUpper(),
            testIdenticalStr2.ToUpper(),
            TextIdenticalModeFlags.DeepCompare,
            false
        );
    }

    private static void TestIdentical(Text a, Text b, TextIdenticalModeFlags flags, bool expected)
    {
        var actualResult = a.IdenticalTo(b, flags);
        if (actualResult != expected)
        {
            Assert.Fail($"new Text(\"{a}\").IdenticalTo(new Text(\"{b}\")) expected={expected} actual={actualResult}");
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

    [Test]
    public void CanGetFormatPatternParameterNames()
    {
        using var scope = Assert.EnterMultipleScope();
        var testText = Text.AsCultureInvariant("My name is {Name}.");
        TestPatternParameterEnumeration(testText, "Name");

        testText = Text.AsCultureInvariant("My age is {Age}.");
        TestPatternParameterEnumeration(testText, "Age");

        testText = Text.AsCultureInvariant("If my age is {Age}, I have been alive for {Age} year(s).");
        TestPatternParameterEnumeration(testText, "Age");

        testText = Text.AsCultureInvariant("{0} - {1} - {2} - {3}");
        TestPatternParameterEnumeration(testText, "0", "1", "2", "3");

        testText = Text.AsCultureInvariant("My name is {Name}. My age is {Age}. My gender is {Gender}.");
        TestPatternParameterEnumeration(testText, "Name", "Age", "Gender");
    }

    private static void TestPatternParameterEnumeration(Text pattern, params ReadOnlySpan<string> expectedParameters)
    {
        Text.Format(pattern, ArgText0, ArgText1, ArgText2, ArgText3);
        var parameters = Text.GetFormatPatternParameters(pattern).ToImmutableArray();
        if (expectedParameters.SequenceEqual(parameters.AsSpan()))
            return;

        var actualParametersString = string.Join(", ", parameters);
        var expectedParametersString = string.Join(", ", expectedParameters);
        Assert.Fail(
            $"\"{pattern}\" contains parameters ({actualParametersString}) but expected ({expectedParametersString})."
        );
    }

    [Test]
    public void CanCompareEquivalentCharactersInDifferentCases()
    {
        using var scope = Assert.EnterMultipleScope();

        TestCompareEqual("a", "A", TextComparisonLevel.IgnoreCaseAccentWidth); // Basic sanity check
        TestCompareEqual("a", "a", TextComparisonLevel.CultureSensitive); // Basic sanity check
        TestCompareEqual("A", "A", TextComparisonLevel.CultureSensitive); // Basic sanity check

        // Test equivalence
        TestCompareEqual("ss", "\x00DF", TextComparisonLevel.IgnoreCaseAccentWidth); // Lowercase Sharp s
        TestCompareEqual("SS", "\x1E9E", TextComparisonLevel.IgnoreCaseAccentWidth); // Uppercase Sharp S
        TestCompareEqual("ae", "\x00E6", TextComparisonLevel.IgnoreCaseAccentWidth); // Lowercase ae
        TestCompareEqual("AE", "\x00C6", TextComparisonLevel.IgnoreCaseAccentWidth); // Uppercase AE

        // Test accentuation
        TestCompareEqual("u", "\x00FC", TextComparisonLevel.IgnoreCaseAccentWidth); // Lowercase u with dieresis
        TestCompareEqual("U", "\x00DC", TextComparisonLevel.IgnoreCaseAccentWidth); // Uppercase U with dieresis

        return;

        void TestCompareEqual(string a, string b, TextComparisonLevel level)
        {
            if (!new Text(a).Equals(new Text(b), level))
            {
                Assert.Fail($"Expected {a} to be equal to {b} at level {level}");
            }
        }
    }

    [Test]
    public void SortingIsAffectedByCulture()
    {
        using var scope = Assert.EnterMultipleScope();

        if (CultureManager.Instance.SetCurrentCulture("fr"))
        {
            ReadOnlySpan<Text> correctedSortedValues =
            [
                Text.AsCultureInvariant("cote"),
                Text.AsCultureInvariant("cot\u00e9"),
                Text.AsCultureInvariant("c\u00f4te"),
                Text.AsCultureInvariant("c\u00f4t\u00e9"),
            ];

            Span<Text> unsortedValues =
            [
                correctedSortedValues[1],
                correctedSortedValues[3],
                correctedSortedValues[2],
                correctedSortedValues[0],
            ];

            unsortedValues.Sort();

            if (!unsortedValues.SequenceEqual(correctedSortedValues))
            {
                Assert.Fail("Sort order is wrong for culture 'fr'.");
            }
        }
        else
        {
            Assert.Warn("Internationalization data for culture 'fr' not found.");
        }

        if (CultureManager.Instance.SetCurrentCulture("fr-CA"))
        {
            ReadOnlySpan<Text> correctedSortedValues =
            [
                Text.AsCultureInvariant("cote"),
                Text.AsCultureInvariant("côte"),
                Text.AsCultureInvariant("coté"),
                Text.AsCultureInvariant("côté"),
            ];

            Span<Text> unsortedValues =
            [
                correctedSortedValues[1],
                correctedSortedValues[3],
                correctedSortedValues[2],
                correctedSortedValues[0],
            ];

            unsortedValues.Sort();

            if (!unsortedValues.SequenceEqual(correctedSortedValues))
            {
                Assert.Fail("Sort order is wrong for culture 'fr-CA'.");
            }
        }
        else
        {
            Assert.Warn("Internationalization data for culture 'fr-CA' not found.");
        }
    }

    [Test]
    public async Task CultureAffectsFormatting()
    {
        var args = new OrderedDictionary<string, FormatArg>
        {
            ["String1"] = LocText("RebuildFTextTest1_Lorem", "Lorem"),
            ["String2"] = LocText("RebuildFTextTest1_Ipsum", "Ipsum"),
        };
        var formattedTest1 = Text.Format(LocText("RebuildNamedText1", "{String1} \"Lorem Ipsum\" {String2}"), args);

        var argsOrdered = ImmutableArray.Create<FormatArg>(
            LocText("RebuildFTextTest1_Lorem", "Lorem"),
            LocText("RebuildFTextTest1_Ipsum", "Ipsum")
        );
        var formattedTestOrdered1 = Text.Format(LocText("RebuildOrderedText1", "{0} \"Lorem Ipsum\" {1}"), argsOrdered);

        var asNumberTest1 = Text.AsNumber(5.5421);

        var asPercentTest1 = Text.AsPercent(0.925);
        var asCurrencyTest1 = Text.AsCurrency(10025, "USD");

        DateTimeOffset dateTimeInfo = new DateTime(2080, 8, 20, 9, 33, 22, DateTimeKind.Utc);
        var asDateTimeTest1 = Text.AsDateTime(dateTimeInfo, timeZoneId: "UTC");

        var argLayers2 = new OrderedDictionary<string, FormatArg>()
        {
            ["NamedLayer1"] = formattedTest1,
            ["OrderedLayer1"] = formattedTestOrdered1,
            ["FTextNumber"] = asNumberTest1,
            ["Number"] = 5010.89221,
            ["DateTime"] = asDateTimeTest1,
            ["Percent"] = asPercentTest1,
            ["Currency"] = asCurrencyTest1,
        };
        var formattedTestLayer2 = Text.Format(
            LocText(
                "RebuildTextLayer2",
                "{NamedLayer1} | {OrderedLayer1} | {FTextNumber} | {Number} | {DateTime} | {Percent} | {Currency}"
            ),
            argLayers2
        );

        formattedTestLayer2.BuildSourceString();

        var task = new TaskCompletionSource();
        var completeTask = () => task.SetResult();
        LocalizationManager.Instance.OnTextRevisionChanged += completeTask;
        try
        {
            // Swap to French-Canadian to check if rebuilding works
            CultureManager.Instance.SetCurrentCulture("fr-CA");

            await task.Task.WaitAsync(TimeSpan.FromSeconds(10));

            _ = formattedTestLayer2.ToString();

            using var scope = Assert.EnterMultipleScope();
            CompareToFrenchCanadian(asNumberTest1, Text.AsNumber(5.5421));
            CompareToFrenchCanadian(asPercentTest1, Text.AsPercent(0.925));
            CompareToFrenchCanadian(asCurrencyTest1, Text.AsCurrency(10025, "USD"));
            CompareToFrenchCanadian(asDateTimeTest1, Text.AsDateTime(dateTimeInfo, timeZoneId: "UTC"));
        }
        finally
        {
            LocalizationManager.Instance.OnTextRevisionChanged -= completeTask;
        }

        return;

        static void CompareToFrenchCanadian(
            Text localized,
            Text invariant,
            [CallerArgumentExpression(nameof(localized))] string caller = "???"
        )
        {
            if (localized.CompareTo(invariant) != 0)
            {
                Assert.Fail($"{caller} did not rebuild correct in French-Canadian\nValue: {localized}");
            }
        }
    }

    private static readonly ImmutableArray<double> InputValues =
    [
        1000.1224,
        1000.1225,
        1000.1226,
        1000.1234,
        1000.1235,
        1000.1236,
        1000.1244,
        1000.1245,
        1000.1246,
        1000.1254,
        1000.1255,
        1000.1256,
        -1000.1224,
        -1000.1225,
        -1000.1226,
        -1000.1234,
        -1000.1235,
        -1000.1236,
        -1000.1244,
        -1000.1245,
        -1000.1246,
        -1000.1254,
        -1000.1255,
        -1000.1256,
    ];

    private static readonly ImmutableArray<RoundingModeTuple> RoundingModeOutputValues =
    [
        ("1000.122", "1000.122", "1000.122", "1000.123", "1000.122", "1000.122", "1000.123"),
        ("1000.122", "1000.123", "1000.122", "1000.123", "1000.122", "1000.122", "1000.123"),
        ("1000.123", "1000.123", "1000.123", "1000.123", "1000.122", "1000.122", "1000.123"),
        ("1000.123", "1000.123", "1000.123", "1000.124", "1000.123", "1000.123", "1000.124"),
        ("1000.124", "1000.124", "1000.123", "1000.124", "1000.123", "1000.123", "1000.124"),
        ("1000.124", "1000.124", "1000.124", "1000.124", "1000.123", "1000.123", "1000.124"),
        ("1000.124", "1000.124", "1000.124", "1000.125", "1000.124", "1000.124", "1000.125"),
        ("1000.124", "1000.125", "1000.124", "1000.125", "1000.124", "1000.124", "1000.125"),
        ("1000.125", "1000.125", "1000.125", "1000.125", "1000.124", "1000.124", "1000.125"),
        ("1000.125", "1000.125", "1000.125", "1000.126", "1000.125", "1000.125", "1000.126"),
        ("1000.126", "1000.126", "1000.125", "1000.126", "1000.125", "1000.125", "1000.126"),
        ("1000.126", "1000.126", "1000.126", "1000.126", "1000.125", "1000.125", "1000.126"),
        ("-1000.122", "-1000.122", "-1000.122", "-1000.123", "-1000.122", "-1000.123", "-1000.122"),
        ("-1000.122", "-1000.123", "-1000.122", "-1000.123", "-1000.122", "-1000.123", "-1000.122"),
        ("-1000.123", "-1000.123", "-1000.123", "-1000.123", "-1000.122", "-1000.123", "-1000.122"),
        ("-1000.123", "-1000.123", "-1000.123", "-1000.124", "-1000.123", "-1000.124", "-1000.123"),
        ("-1000.124", "-1000.124", "-1000.123", "-1000.124", "-1000.123", "-1000.124", "-1000.123"),
        ("-1000.124", "-1000.124", "-1000.124", "-1000.124", "-1000.123", "-1000.124", "-1000.123"),
        ("-1000.124", "-1000.124", "-1000.124", "-1000.125", "-1000.124", "-1000.125", "-1000.124"),
        ("-1000.124", "-1000.125", "-1000.124", "-1000.125", "-1000.124", "-1000.125", "-1000.124"),
        ("-1000.125", "-1000.125", "-1000.125", "-1000.125", "-1000.124", "-1000.125", "-1000.124"),
        ("-1000.125", "-1000.125", "-1000.125", "-1000.126", "-1000.125", "-1000.126", "-1000.125"),
        ("-1000.126", "-1000.126", "-1000.125", "-1000.126", "-1000.125", "-1000.126", "-1000.125"),
        ("-1000.126", "-1000.126", "-1000.126", "-1000.126", "-1000.125", "-1000.126", "-1000.125"),
    ];

    private static IEnumerable RoundModeTestData()
    {
        return InputValues
            .Zip(RoundingModeOutputValues)
            .SelectMany(tuple =>
            {
                return new[]
                {
                    (tuple.First, RoundingMode.HalfToEven, tuple.Second.HalfToEven),
                    (tuple.First, RoundingMode.HalfFromZero, tuple.Second.HalfFromZero),
                    (tuple.First, RoundingMode.HalfToZero, tuple.Second.HalfToZero),
                    (tuple.First, RoundingMode.FromZero, tuple.Second.FromZero),
                    (tuple.First, RoundingMode.ToZero, tuple.Second.ToZero),
                    (tuple.First, RoundingMode.ToNegativeInfinity, tuple.Second.ToNegativeInfinity),
                    (tuple.First, RoundingMode.ToPositiveInfinity, tuple.Second.ToPositiveInfinity),
                };
            })
            .Concat([
                (1000.12459, RoundingMode.HalfToEven, "1000.125"),
                (1000.124549, RoundingMode.HalfToEven, "1000.125"),
                (1000.124551, RoundingMode.HalfToEven, "1000.125"),
                (1000.12451, RoundingMode.HalfToEven, "1000.125"),
                (1000.1245000001, RoundingMode.HalfToEven, "1000.124"),
                (1000.12450000000001, RoundingMode.HalfToEven, "1000.124"),
                (512.9999, RoundingMode.HalfToEven, "513"),
                (-512.9999, RoundingMode.HalfToEven, "-513"),
            ])
            .Select(t => new TestCaseData(t.Item1, t.Item2, t.Item3)
            {
                TestName = $"RoundMode={t.Item2} Input={t.Item1}",
            });
    }

    [Test]
    [TestCaseSource(nameof(RoundModeTestData))]
    public void TextRoundingModesBehaveCorrectly(double input, RoundingMode roundingMode, string expectedOutput)
    {
        using var scope = Assert.EnterMultipleScope();

        CultureManager.Instance.SetCurrentCulture("en");

        var formattingOptions = new NumberFormattingOptions
        {
            UseGrouping = false,
            MinimumFractionalDigits = 0,
            MaximumFractionalDigits = 3,
            RoundingMode = roundingMode,
        };

        Assert.That(Text.AsNumber(input, formattingOptions).ToString(), Is.EqualTo(expectedOutput));
    }

    private static IEnumerable PaddingTestData()
    {
        var formattingOptions = new NumberFormattingOptions { UseGrouping = false, MaximumIntegralDigits = 3 };
        yield return new TestCaseData<NumberFormattingOptions, int, string>(formattingOptions, 123456, "456")
        {
            TestName = "Truncating '123456' to a max of 3 integral digits",
            TypeArgs = [typeof(int)],
        };
        yield return new TestCaseData<NumberFormattingOptions, int, string>(formattingOptions, -123456, "-456")
        {
            TestName = "Truncating '-123456' to a max of 3 integral digits",
            TypeArgs = [typeof(int)],
        };

        formattingOptions = new NumberFormattingOptions() { UseGrouping = false, MinimumIntegralDigits = 6 };
        yield return new TestCaseData<NumberFormattingOptions, int, string>(formattingOptions, 123, "000123")
        {
            TestName = "Padding '123' to a min of 6 integral digits",
            TypeArgs = [typeof(int)],
        };
        yield return new TestCaseData<NumberFormattingOptions, int, string>(formattingOptions, -123, "-000123")
        {
            TestName = "Padding '-123' to a min of 6 integral digits",
            TypeArgs = [typeof(int)],
        };

        formattingOptions = new NumberFormattingOptions() { UseGrouping = false, MinimumFractionalDigits = 3 };
        yield return new TestCaseData<NumberFormattingOptions, int, string>(formattingOptions, 123, "123.000")
        {
            TestName = "Padding '123' to a min of 3 fractional digits",
            TypeArgs = [typeof(int)],
        };
        yield return new TestCaseData<NumberFormattingOptions, int, string>(formattingOptions, -123, "-123.000")
        {
            TestName = "Padding '-123' to a min of 3 fractional digits",
            TypeArgs = [typeof(int)],
        };

        formattingOptions = new NumberFormattingOptions() { UseGrouping = false, MaximumFractionalDigits = 4 };
        yield return new TestCaseData<NumberFormattingOptions, double, string>(formattingOptions, 0.00123, "0.0012")
        {
            TestName = "Padding '0.00123' to a max of 4 fractional digits",
            TypeArgs = [typeof(double)],
        };
        yield return new TestCaseData<NumberFormattingOptions, double, string>(formattingOptions, -0.00123, "-0.0012")
        {
            TestName = "Padding '-0.00123' to a max of 4 fractional digits",
            TypeArgs = [typeof(double)],
        };

        formattingOptions = new NumberFormattingOptions()
        {
            UseGrouping = false,
            MinimumFractionalDigits = 8,
            MaximumFractionalDigits = 8,
        };
        yield return new TestCaseData<NumberFormattingOptions, double, string>(formattingOptions, 0.00123, "0.00123000")
        {
            TestName = "Padding '0.00123' to a max of 8 fractional digits",
            TypeArgs = [typeof(double)],
        };
        yield return new TestCaseData<NumberFormattingOptions, double, string>(
            formattingOptions,
            -0.00123,
            "-0.00123000"
        )
        {
            TestName = "Padding '-0.00123' to a max of 8 fractional digits",
            TypeArgs = [typeof(double)],
        };
    }

    [Test]
    [TestCaseSource(nameof(PaddingTestData))]
    public void TextPaddingShouldBehaveCorrectly<T>(NumberFormattingOptions options, T input, string expectedOutput)
        where T : unmanaged, INumber<T>
    {
        CultureManager.Instance.SetCurrentCulture("en");
        var resultText = Text.AsNumber(FormatNumericArg.FromNumber(input), options);
        Assert.That(resultText.ToString(), Is.EqualTo(expectedOutput));
    }

    private static IEnumerable<TestCaseData<Text, string>> StringificationTextData()
    {
        yield return new TestCaseData<Text, string>(
            NsLocText("Core.Tests.TextFormatTest", "TextStringificationTest_Lorem", "Lorem"),
            "NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Lorem\", \"Lorem\")"
        )
        {
            TestName = "Stringification of a Text object",
        };
        yield return new TestCaseData<Text, string>(
            LocText("TextStringificationTest_Lorem", "Lorem"),
            "NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Lorem\", \"Lorem\")"
        )
        {
            TestName = "Stringification of a LocText object",
        };
        /*
        yield return new TestCaseData<Text, string>(
            LocTable("Core.Tests.TextFormatTest", "TextStringificationTest_Lorem"),
            "LOCTABLE(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Lorem\")")
        {
            TestName = "Stringification of a LocTable object",
        };
        */
        yield return new TestCaseData<Text, string>(InvText("DummyText"), "INVTEXT(\"DummyText\")")
        {
            TestName = "Stringification of an InvText object",
        };

        yield return new TestCaseData<Text, string>(LocGenNumber(10, ""), "LOCGEN_NUMBER(10, \"\")")
        {
            TestName = "Stringification of a LocGenNumber object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenNumberGrouped(12.5f, ""),
            "LOCGEN_NUMBER_GROUPED(12.5f, \"\")"
        )
        {
            TestName = "Stringification of a LocGenNumberGrouped object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenNumberUngrouped(12.5f, ""),
            "LOCGEN_NUMBER_UNGROUPED(12.5f, \"\")"
        )
        {
            TestName = "Stringification of a LocGenNumberUngrouped object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenNumberCustom(
                +10,
                new NumberFormattingOptions()
                {
                    AlwaysSign = true,
                    RoundingMode = RoundingMode.ToZero,
                    MinimumFractionalDigits = 2,
                },
                ""
            ),
            "LOCGEN_NUMBER_CUSTOM(10, SetAlwaysSign(true).SetRoundingMode(ERoundingMode::ToZero).SetMinimumFractionalDigits(2), \"\")"
        )
        {
            TestName = "Stringification of a LocGenNumberCustom object",
        };
        yield return new TestCaseData<Text, string>(LocGenNumber(-10, "en"), "LOCGEN_NUMBER(-10, \"en\")")
        {
            TestName = "Stringification of a LocGenNumber object with a culture",
        };

        yield return new TestCaseData<Text, string>(LocGenPercent(0.1f, ""), "LOCGEN_PERCENT(0.1f, \"\")")
        {
            TestName = "Stringification of a LocGenPercent object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenPercentGrouped(0.1f, ""),
            "LOCGEN_PERCENT_GROUPED(0.1f, \"\")"
        )
        {
            TestName = "Stringification of a LocGenPercentGrouped object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenPercentUngrouped(0.1f, ""),
            "LOCGEN_PERCENT_UNGROUPED(0.1f, \"\")"
        )
        {
            TestName = "Stringification of a LocGenPercentUngrouped object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenPercentCustom(
                0.1f,
                new NumberFormattingOptions()
                {
                    AlwaysSign = true,
                    RoundingMode = RoundingMode.ToZero,
                    MinimumFractionalDigits = 2,
                },
                ""
            ),
            "LOCGEN_PERCENT_CUSTOM(0.1f, SetAlwaysSign(true).SetRoundingMode(ERoundingMode::ToZero).SetMinimumFractionalDigits(2), \"\")"
        )
        {
            TestName = "Stringification of a LocGenPercentCustom object",
        };
        yield return new TestCaseData<Text, string>(LocGenPercent(0.1, "en"), "LOCGEN_PERCENT(0.1, \"en\")")
        {
            TestName = "Stringification of a LocGenPercent object with a culture",
        };

        yield return new TestCaseData<Text, string>(
            LocGenCurrency(125, "USD", ""),
            "LOCGEN_CURRENCY(125, \"USD\", \"\")"
        )
        {
            TestName = "Stringification of a LocGenCurrency object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenCurrency(125, "USD", "en"),
            "LOCGEN_CURRENCY(125, \"USD\", \"en\")"
        )
        {
            TestName = "Stringification of a LocGenCurrency object with a culture",
        };

        yield return new TestCaseData<Text, string>(
            LocGenDateUtc(1526342400, DateTimeFormatStyle.Short, "", "en-GB"),
            "LOCGEN_DATE_UTC(1526342400, EDateTimeStyle::Short, \"\", \"en-GB\")"
        )
        {
            TestName = "Stringification of a LocGenDateUtc object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenDateLocal(1526342400, DateTimeFormatStyle.Medium, ""),
            "LOCGEN_DATE_LOCAL(1526342400, EDateTimeStyle::Medium, \"\")"
        )
        {
            TestName = "Stringification of a LocGenDateLocal object",
        };

        yield return new TestCaseData<Text, string>(
            LocGenTimeUtc(1526342400, DateTimeFormatStyle.Long, "", "en-GB"),
            "LOCGEN_TIME_UTC(1526342400, EDateTimeStyle::Long, \"\", \"en-GB\")"
        )
        {
            TestName = "Stringification of a LocGenTimeUtc object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenTimeLocal(1526342400, DateTimeFormatStyle.Full, ""),
            "LOCGEN_TIME_LOCAL(1526342400, EDateTimeStyle::Full, \"\")"
        )
        {
            TestName = "Stringification of a LocGenTimeLocal object",
        };

        yield return new TestCaseData<Text, string>(
            LocGenDateTimeUtc(1526342400, DateTimeFormatStyle.Short, DateTimeFormatStyle.Medium, "", "en-GB"),
            "LOCGEN_DATETIME_UTC(1526342400, EDateTimeStyle::Short, EDateTimeStyle::Medium, \"\", \"en-GB\")"
        )
        {
            TestName = "Stringification of a LocGenDateTimeUtc object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenDateTimeLocal(1526342400, DateTimeFormatStyle.Long, DateTimeFormatStyle.Full, ""),
            "LOCGEN_DATETIME_LOCAL(1526342400, EDateTimeStyle::Long, EDateTimeStyle::Full, \"\")"
        )
        {
            TestName = "Stringification of a LocGenDateTimeLocal object",
        };

        yield return new TestCaseData<Text, string>(
            LocGenDateTimeCustomUtc(1526342400, "%A, %B %e, %Y", "", "en-GB"),
            "LOCGEN_DATETIME_CUSTOM_UTC(1526342400, \"%A, %B %e, %Y\", \"\", \"en-GB\")"
        )
        {
            TestName = "Stringification of a LocGenDateTimeCustomUtc object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenDateTimeCustomLocal(1526342400, "%A, %B %e, %Y", ""),
            "LOCGEN_DATETIME_CUSTOM_LOCAL(1526342400, \"%A, %B %e, %Y\", \"\")"
        )
        {
            TestName = "Stringification of a LocGenDateTimeCustomLocal object",
        };

        yield return new TestCaseData<Text, string>(
            LocGenToUpper(LocText("TextStringificationTest_Lorem", "Lorem")),
            "LOCGEN_TOUPPER(NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Lorem\", \"Lorem\"))"
        )
        {
            TestName = "Stringification of a LocGenToUpper object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenToLower(LocText("TextStringificationTest_Lorem", "Lorem")),
            "LOCGEN_TOLOWER(NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Lorem\", \"Lorem\"))"
        )
        {
            TestName = "Stringification of a LocGenToLower object",
        };

        yield return new TestCaseData<Text, string>(
            LocGenFormatOrdered(
                LocText("TextStringificationTest_FmtO", "{0} weighs {1}kg"),
                LocText("TextStringificationTest_Bear", "Bear"),
                227
            ),
            "LOCGEN_FORMAT_ORDERED(NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_FmtO\", "
                + "\"{0} weighs {1}kg\"), NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Bear\", \"Bear\"), 227)"
        )
        {
            TestName = "Stringification of a LocGenFormatOrdered object",
        };
        yield return new TestCaseData<Text, string>(
            LocGenFormatNamed(
                LocText("TextStringificationTest_FmtN", "{Animal} weighs {Weight}kg"),
                ("Animal", LocText("TextStringificationTest_Bear", "Bear")),
                ("Weight", 227)
            ),
            "LOCGEN_FORMAT_NAMED(NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_FmtN\", \"{Animal} weighs {Weight}kg\"), \"Animal\", NSLOCTEXT(\"Core.Tests.TextFormatTest\", \"TextStringificationTest_Bear\", \"Bear\"), \"Weight\", 227)"
        )
        {
            TestName = "Stringification of a LocGenFormatNamed object",
        };
    }

    [Test]
    [TestCaseSource(nameof(StringificationTextData))]
    public void TextCanBeStringifiedEasily(Text expectedText, string expectedString)
    {
        CultureManager.Instance.SetCurrentCulture("en-US");

        var builder = new StringBuilder();
        TextStringHelper.WriteToBuffer(builder, expectedText);
        Assert.That(builder.ToString(), Is.EqualTo(expectedString));

        var actualText = TextStringHelper.ReadFromBuffer(expectedString);
        Assert.That(actualText, Is.EqualTo(expectedText));
    }
}
