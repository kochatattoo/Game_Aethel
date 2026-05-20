namespace ZLinq.Tests.Linq;

public class RangeSystemRangeTest
{
    [Fact]
    public void BasicRangeTests()
    {
        // Test with exclusive right bound (default)
        var range1 = ValueEnumerable.Range(0..5).ToArray();
        range1.ShouldBe(new[] { 0, 1, 2, 3, 4 });

        // Test with inclusive right bound
        var range2 = ValueEnumerable.Range(0..5, RightBound.Inclusive).ToArray();
        range2.ShouldBe(new[] { 0, 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void EmptyRangeTests()
    {
        var emptyRange = ValueEnumerable.Range(5..5).ToArray();
        emptyRange.ShouldBeEmpty();

        var singleItem = ValueEnumerable.Range(5..5, RightBound.Inclusive).ToArray();
        singleItem.ShouldBe(new[] { 5 });
    }

    [Fact]
    public void InvalidRangeTests()
    {
        // Test FromEnd index at start
        Should.Throw<Exception>(() => ValueEnumerable.Range(^1..5));

        // Test FromEnd index at end (not zero)
        Should.Throw<Exception>(() => ValueEnumerable.Range(0..^1));

        // End smaller than start
        Should.Throw<ArgumentOutOfRangeException>(() => ValueEnumerable.Range(5..3));
    }

    [Fact]
    public void InfiniteRangeTest()
    {
        // Test range with end at ^0 (which should create an "infinite" sequence)
        var infiniteRange = ValueEnumerable.Range(1..^0);

        // Take first 1000 elements to verify it works
        var first1000 = infiniteRange.Take(1000).ToArray();
        first1000.Length.ShouldBe(1000);

        // First elements should match regular range
        for (int i = 0; i < 1000; i++)
        {
            first1000[i].ShouldBe(i + 1);
        }

        // Verify TryGetNonEnumeratedCount returns false
        var enumerator = infiniteRange.Enumerator;
        enumerator.TryGetNonEnumeratedCount(out int count).ShouldBeFalse();
    }

    [Fact]
    public void NonEnumeratedCountTest()
    {
        var range = ValueEnumerable.Range(0..10);
        var enumerator = range.Enumerator;

        enumerator.TryGetNonEnumeratedCount(out int count).ShouldBeTrue();
        count.ShouldBe(10);
    }
}
