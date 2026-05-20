namespace ZLinq.Tests.Linq;

#if NET8_0_OR_GREATER

public class ToFrozenSetTest
{
    // Pattern follows existing test files:
    // Span = Array
    // EnumeratedCount Only = Select
    // Iterator = ToValueEnumerable

    [Fact]
    public void Empty()
    {
        var xs = new int[0];

        var actual1 = xs.AsValueEnumerable().ToFrozenSet();
        var actual2 = xs.AsValueEnumerable().Select(x => x).ToFrozenSet();
        var actual3 = xs.ToValueEnumerable().ToFrozenSet();

        actual1.Count.ShouldBe(0);
        actual2.Count.ShouldBe(0);
        actual3.Count.ShouldBe(0);
    }

    [Fact]
    public void NonEmpty()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };

        var actual1 = xs.AsValueEnumerable().ToFrozenSet();
        var actual2 = xs.AsValueEnumerable().Select(x => x).ToFrozenSet();
        var actual3 = xs.ToValueEnumerable().ToFrozenSet();

        Assert.Equal(xs.Length, actual1.Count);
        Assert.Equal(xs.Length, actual2.Count);
        Assert.Equal(xs.Length, actual3.Count);

        foreach (var x in xs)
        {
            Assert.Contains(x, actual1.ToHashSet());
            Assert.Contains(x, actual2.ToHashSet());
            Assert.Contains(x, actual3.ToHashSet());
        }
    }

    [Fact]
    public void Comparer()
    {
        string?[] xs = ["foo", "bar", "baz"];

        var set = xs.AsValueEnumerable().ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        set.Count.ShouldBe(3);
        set.Contains("foO").ShouldBeTrue();
        set.Contains("BaR").ShouldBeTrue();
        set.Contains("Baz").ShouldBeTrue();
    }

    [Fact]
    public void DuplicateElements()
    {
        var xs = new int[] { 1, 2, 2, 3, 3, 3 };

        var actual = xs.AsValueEnumerable().ToFrozenSet();

        actual.Count.ShouldBe(3);
        actual.Contains(1).ShouldBeTrue();
        actual.Contains(2).ShouldBeTrue();
        actual.Contains(3).ShouldBeTrue();
    }
}

#endif
