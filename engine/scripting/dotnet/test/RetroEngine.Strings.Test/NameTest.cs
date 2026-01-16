// @file NameTest.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
namespace RetroEngine.Strings.Test;

public class NameTest
{
    [Test]
    public void TestDefaultNameState()
    {
        var name = new Name();
        using var scope = Assert.EnterMultipleScope();
        Assert.That(name.IsNone);
        Assert.That(name.IsValid);
        Assert.That(name.ComparisonIndex.Value, Is.Zero);
        Assert.That(name.DisplayIndex.Value, Is.Zero);
        Assert.That(name.Number, Is.EqualTo(Name.NoNumber));
    }

    [Test]
    public void TestComparisonIndexIsCaseless()
    {
        // Case-insensitive comparison index, but equality uses comparison_index + number.
        Name upper = "Player";
        Name lower = "player";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(upper.IsValid);
            Assert.That(lower.IsValid);

            // Same logical name => same comparison index
            Assert.That(upper.ComparisonIndex, Is.EqualTo(lower.ComparisonIndex));

            // Display strings are case-sensitive; they may differ
            Assert.That(upper.DisplayIndex, Is.Not.Zero);
            Assert.That(lower.DisplayIndex, Is.Not.Zero);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(upper.DisplayIndex, Is.Not.EqualTo(lower.DisplayIndex));

            // Equality operator only cares about comparison_index + number
            Assert.That(upper, Is.EqualTo(lower));
        }
    }

    [Test]
    public void TestNumberSuffix()
    {
        Name n = "Enemy_42";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(n.IsValid);
            Assert.That(n.IsNone, Is.False);

            // Number gets parsed out
            Assert.That(n.Number, Is.EqualTo(42));
        }

        using (Assert.EnterMultipleScope())
        {
            // Stored base name should be with the _42 suffix
            var baseValue = n.ToString();
            Assert.That(baseValue, Is.EqualTo("Enemy_42"));

            // Comparison is still against the base logical name
            Assert.That(n, Is.EqualTo("Enemy_42"));
        }
    }

    [Test]
    public void TestIgnoreInvalidNumericSuffix()
    {
        // Leading zero after underscore should be rejected as a number
        Name withLeadingZero = "Foo_01";
        using (Assert.EnterMultipleScope())
        {
            Assert.That(withLeadingZero.IsValid, Is.True);
            Assert.That(withLeadingZero.Number, Is.EqualTo(Name.NoNumber));
            Assert.That(withLeadingZero.ToString(), Is.EqualTo("Foo_01"));
        }

        // No underscore before digits -> treated as part of the name
        Name noUnderscore = "Bar99";
        using (Assert.EnterMultipleScope())
        {
            Assert.That(noUnderscore.IsValid, Is.True);
            Assert.That(noUnderscore.Number, Is.EqualTo(Name.NoNumber));
            Assert.That(noUnderscore.ToString(), Is.EqualTo("Bar99"));
        }
    }

    [Test]
    public void FindNameDoesNotCreateNewEntries()
    {
        // First ensure there is one known entry
        Name existing = "Knight";

        Assert.That(existing.IsValid);
        var existingComparison = existing.ComparisonIndex;
        var existingDisplay = existing.DisplayIndex;

        // Lookup again using FindType::Find -> should find the same indices
        var foundExisting = new Name("Knight", FindName.Find);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(foundExisting.IsValid);
            Assert.That(foundExisting.IsNone, Is.False);
            Assert.That(foundExisting.ComparisonIndex, Is.EqualTo(existingComparison));
            Assert.That(foundExisting.DisplayIndex, Is.EqualTo(existingDisplay));
        }

        // Lookup unknown name with FindType::Find -> should yield a "none" name
        var notCreated = new Name("UnknownNameThatDoesNotExist", FindName.Find);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(notCreated.IsNone);
            Assert.That(notCreated.IsValid);
            Assert.That(notCreated.ComparisonIndex.Value, Is.Zero);
            Assert.That(notCreated.DisplayIndex.Value, Is.Zero);
        }
    }

    [Test]
    public void TestEqualsComparisonCaseInsensitive()
    {
        Name n = "Boss";
        using (Assert.EnterMultipleScope())
        {
            Assert.That(n.IsValid);
            Assert.That(n, Is.EqualTo("boss"));
            Assert.That(n, Is.EqualTo("BOSS"));
            Assert.That(n, Is.Not.EqualTo("miniboss"));
        }
    }

    [Test]
    public void TestNameNone()
    {
        var none = Name.None;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(none.IsNone);
            Assert.That(none.IsValid);
            Assert.That(none.ComparisonIndex.Value, Is.Zero);
            Assert.That(none.DisplayIndex.Value, Is.Zero);
            Assert.That(none.Number, Is.EqualTo(Name.NoNumber));
        }

        var noneString = none.ToString();
        Assert.That(noneString, Is.EqualTo("None"));
    }
}
