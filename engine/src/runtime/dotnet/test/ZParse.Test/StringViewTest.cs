namespace ZParse.Test;

public class StringViewTest
{
    [Test]
    public void DefaultViewHasNoValue()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var view = default(StringView);
            view.AsSpan();
        });
    }

    [Test]
    public void IdenticalViewsAreEqual()
    {
        const string source = "123";
        var t1 = new StringView(source, TextPosition.Zero, 1);
        var t2 = new StringView(source, TextPosition.Zero, 1);
        Assert.That(t1 == t2);
    }

    [Test]
    public void ViewsFromDifferentSourcesAreNotEqual()
    {
        const string source1 = "123";
        var source2 = "1234"[..3];
        var t1 = new StringView(source1, TextPosition.Zero, 1);
        var t2 = new StringView(source2, TextPosition.Zero, 1);
        Assert.That(t1 != t2);
    }

    [Test]
    public void DifferentLengthViewsAreNotEqual()
    {
        const string source = "123";
        var t1 = new StringView(source, TextPosition.Zero, 1);
        var t2 = new StringView(source, TextPosition.Zero, 2);
        Assert.That(t1 != t2);
    }

    [Test]
    public void EqualSpansAreEqualCase65()
    {
        const string source = "123";
        var one = TextPosition.Zero.Advance(source[0]);
        var t1 = new StringView(source);
        var t2 = new StringView(source, one, 1);
        Assert.That(t1.Until(t2).ToString(), Is.EqualTo("1"));
    }

    [Test]
    public void SpansAtDifferentPositionsAreNotEqual()
    {
        const string source = "111";
        var t1 = new StringView(source, TextPosition.Zero, 1);
        var t2 = new StringView(source, new TextPosition(1, 1, 1), 1);
        Assert.That(t1 != t2);
    }

    [Test]
    [TestCase("Hello", 0, 5, "Hello")]
    [TestCase("Hello", 1, 4, "ello")]
    [TestCase("Hello", 1, 3, "ell")]
    [TestCase("Hello", 0, 0, "")]
    public void ASpanIsEqualInValueToAMatchingString(string str, int offset, int length, string value)
    {
        var span = new StringView(str, new TextPosition(offset, 1, offset + 1), length);
        Assert.That(span.Equals(value, StringComparison.Ordinal));
    }

    [Test]
    [TestCase("Hello", 0, 5, "HELLO")]
    [TestCase("Hello", 1, 4, "ELLO")]
    [TestCase("Hello", 1, 3, "ELL")]
    [TestCase("Hello", 0, 0, "")]
    public void ASpanIsEqualInValueIgnoringCaseToAMatchingUppsercaseString(
        string str,
        int offset,
        int length,
        string value
    )
    {
        var span = new StringView(str, new TextPosition(offset, 1, offset + 1), length);
        Assert.That(span.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    [TestCase("Hello", 0, 5, "HELLO")]
    [TestCase("Hello", 1, 4, "Hell")]
    [TestCase("Hello", 1, 3, "fll")]
    public void ASpanIsNotEqualToADifferentString(string str, int offset, int length, string value)
    {
        var span = new StringView(str, new TextPosition(offset, 1, offset + 1), length);
        Assert.That(span.Equals(value, StringComparison.Ordinal), Is.False);
    }

    [Test]
    [TestCase("Hello", 0, 5)]
    [TestCase("Hello", 0, 3)]
    [TestCase("Hello", 1, 3)]
    public void SliceWithLengthExtractsCorrectCharacters(string input, int index, int end)
    {
        var inputSpan = new StringView(input, new TextPosition(0, 1, 1), input.Length);
        var slice = inputSpan[index..end];
        Assert.That(slice.ToString(), Is.EqualTo(input[index..end]));
    }

    [Test]
    [TestCase("Hello", 0)]
    [TestCase("Hello", 2)]
    [TestCase("Hello", 5)]
    public void SliceWithoutLengthExtractsCorrectCharacters(string input, int index)
    {
        var inputSpan = new StringView(input, new TextPosition(0, 1, 1), input.Length);
        var slice = inputSpan[index..];
        Assert.That(slice.ToString(), Is.EqualTo(input[index..]));
    }

    [Test]
    [TestCase("Hello", 0)]
    [TestCase("Hello", 2)]
    [TestCase("Hello", 4)]
    public void IndexerExtractsCorrectCharacter(string input, int index)
    {
        var inputSpan = new StringView(input, new TextPosition(0, 1, 1), input.Length);
        var ch = inputSpan[index];
        Assert.That(ch, Is.EqualTo(input[index]));
    }
}
