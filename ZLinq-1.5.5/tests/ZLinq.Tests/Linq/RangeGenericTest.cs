#if NET8_0_OR_GREATER
using System.Numerics;

namespace ZLinq.Tests.Linq;

public class RangeGenericTest
{
    [Fact]
    public void GenericIntegerTest()
    {
        // Test Range<int> with default step (1)
        var range1 = ValueEnumerable.Range<int>(5, 10).ToArray();
        range1.ShouldBe(Enumerable.Range(5, 10).ToArray());

        // Test Range<int> with custom step
        var range2 = ValueEnumerable.Range<int, int>(5, 5, 2).ToArray();
        range2.ShouldBe(new[] { 5, 7, 9, 11, 13 });
    }

    [Fact]
    public void GenericRangeToTest()
    {
        // Test Range<int> with end value, exclusive bound
        var range1 = ValueEnumerable.Range<int>(1, 10, RightBound.Exclusive).ToArray();
        range1.ShouldBe(Enumerable.Range(1, 9).ToArray());

        // Test Range<int> with end value, inclusive bound
        var range2 = ValueEnumerable.Range<int>(1, 10, RightBound.Inclusive).ToArray();
        range2.ShouldBe(Enumerable.Range(1, 10).ToArray());

        // Test Range<int> with custom step, exclusive bound
        var range3 = ValueEnumerable.Range<int, int>(1, 10, 2, RightBound.Exclusive).ToArray();
        range3.ShouldBe(new[] { 1, 3, 5, 7, 9 });

        // Test Range<int> with custom step, inclusive bound
        var range4 = ValueEnumerable.Range<int, int>(1, 11, 2, RightBound.Inclusive).ToArray();
        range4.ShouldBe(new[] { 1, 3, 5, 7, 9, 11 });
    }

    [Fact]
    public void GenericLongTest()
    {
        // Test with long values
        var range1 = ValueEnumerable.Range<long>(5L, 5).ToArray();
        range1.ShouldBe(new[] { 5L, 6L, 7L, 8L, 9L });

        // Test with custom step
        var range2 = ValueEnumerable.Range<long, long>(5L, 5, 10L).ToArray();
        range2.ShouldBe(new[] { 5L, 15L, 25L, 35L, 45L });

        // Test with end value
        var range3 = ValueEnumerable.Range<long>(5L, 50L, RightBound.Exclusive).ToArray();
        range3.Length.ShouldBe(45);
        range3[0].ShouldBe(5L);
        range3[^1].ShouldBe(49L);
    }

    [Fact]
    public void GenericFloatTest()
    {
        // Test with float values, using count
        var range1 = ValueEnumerable.Range<float>(1.5f, 5).ToArray();
        range1.ShouldBe(new[] { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f });

        // Test with custom step
        var range2 = ValueEnumerable.Range<float, float>(1.0f, 5, 0.5f).ToArray();
        range2.ShouldBe(new[] { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f });

        // Test with end value, exclusive
        var range3 = ValueEnumerable.Range<float>(1.0f, 3.0f, RightBound.Exclusive).ToArray();
        range3.Length.ShouldBe(2); // Since 1.0f + 1.0f = 2.0f, then 2.0f < 3.0f, but 3.0f !< 3.0f
        range3.ShouldBe(new[] { 1.0f, 2.0f });

        // Test with end value, custom step, inclusive
        var range4 = ValueEnumerable.Range<float, float>(1.0f, 2.0f, 0.5f, RightBound.Inclusive).ToArray();
        range4.ShouldBe(new[] { 1.0f, 1.5f, 2.0f });
    }

    [Fact]
    public void GenericDecimalTest()
    {
        // Test with decimal values
        var range1 = ValueEnumerable.Range<decimal>(1.5m, 3).ToArray();
        range1.ShouldBe(new[] { 1.5m, 2.5m, 3.5m });

        // Test with custom step
        var range2 = ValueEnumerable.Range<decimal, decimal>(1.0m, 3, 0.5m).ToArray();
        range2.ShouldBe(new[] { 1.0m, 1.5m, 2.0m });
    }

    [Fact]
    public void EmptyAndEdgeCases()
    {
        // Empty range with count 0
        var empty = ValueEnumerable.Range<int>(0, 0).ToArray();
        empty.ShouldBeEmpty();

        // Single item range
        var single = ValueEnumerable.Range<int>(42, 1).ToArray();
        single.ShouldBe(new[] { 42 });

        // Negative step
        var backwards = ValueEnumerable.Range<int, int>(10, count: 5, -1).ToArray();
        backwards.ShouldBe(new[] { 10, 9, 8, 7, 6 });
    }
}
#endif
