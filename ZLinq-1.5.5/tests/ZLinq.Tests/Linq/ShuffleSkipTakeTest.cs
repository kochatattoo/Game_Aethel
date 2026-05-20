namespace ZLinq.Tests.Linq;

public class ShuffleSkipTakeTest
{
    [Fact]
    public void Take_After_Shuffle()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // Basic take
        var result = source.AsValueEnumerable().Shuffle().Take(5).ToArray();
        result.Length.ShouldBe(5);

        // Take with zero count
        result = source.AsValueEnumerable().Shuffle().Take(0).ToArray();
        result.Length.ShouldBe(0);

        // Take with negative count (should be treated as zero)
        result = source.AsValueEnumerable().Shuffle().Take(-3).ToArray();
        result.Length.ShouldBe(0);

        // Take all elements
        result = source.AsValueEnumerable().Shuffle().Take(source.Length).ToArray();
        result.Length.ShouldBe(source.Length);

        // Take more than available
        result = source.AsValueEnumerable().Shuffle().Take(source.Length + 5).ToArray();
        result.Length.ShouldBe(source.Length);
    }

    [Fact]
    public void TakeLast_After_Shuffle()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // TakeLast is equivalent to Take for shuffled collections
        var result = source.AsValueEnumerable().Shuffle().TakeLast(5).ToArray();
        result.Length.ShouldBe(5);

        // TakeLast with zero count
        result = source.AsValueEnumerable().Shuffle().TakeLast(0).ToArray();
        result.Length.ShouldBe(0);

        // TakeLast with negative count (should be treated as zero)
        result = source.AsValueEnumerable().Shuffle().TakeLast(-3).ToArray();
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void Skip_After_Shuffle()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // Basic skip
        var result = source.AsValueEnumerable().Shuffle().Skip(3).ToArray();
        result.Length.ShouldBe(source.Length - 3);

        // Skip zero elements
        result = source.AsValueEnumerable().Shuffle().Skip(0).ToArray();
        result.Length.ShouldBe(source.Length);

        // Skip negative count (should be treated as zero)
        result = source.AsValueEnumerable().Shuffle().Skip(-5).ToArray();
        result.Length.ShouldBe(source.Length);

        // Skip all elements
        result = source.AsValueEnumerable().Shuffle().Skip(source.Length).ToArray();
        result.Length.ShouldBe(0);

        // Skip more than available
        result = source.AsValueEnumerable().Shuffle().Skip(source.Length + 5).ToArray();
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void SkipLast_After_Shuffle()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // SkipLast is equivalent to Skip for shuffled collections
        var result = source.AsValueEnumerable().Shuffle().SkipLast(3).ToArray();
        result.Length.ShouldBe(source.Length - 3);

        // SkipLast zero elements
        result = source.AsValueEnumerable().Shuffle().SkipLast(0).ToArray();
        result.Length.ShouldBe(source.Length);

        // SkipLast negative count (should be treated as zero)
        result = source.AsValueEnumerable().Shuffle().SkipLast(-5).ToArray();
        result.Length.ShouldBe(source.Length);
    }

    [Fact]
    public void Chained_Skip_Take_Operations()
    {
        var source = Enumerable.Range(1, 20).ToArray();

        // Skip then Take
        var result = source.AsValueEnumerable().Shuffle().Skip(5).Take(10).ToArray();
        result.Length.ShouldBe(10);

        // Take then Skip
        result = source.AsValueEnumerable().Shuffle().Take(15).Skip(5).ToArray();
        var reference = source.Take(15).Skip(5).Count();
        result.Length.ShouldBe(reference); // 10

        // Multiple Skip operations
        result = source.AsValueEnumerable().Shuffle().Skip(3).Skip(4).Skip(2).ToArray();
        result.Length.ShouldBe(source.Length - 9);

        // Multiple Take operations
        result = source.AsValueEnumerable().Shuffle().Take(15).Take(12).Take(10).ToArray();
        result.Length.ShouldBe(10);

        // Skip and SkipLast
        result = source.AsValueEnumerable().Shuffle().Skip(5).SkipLast(5).ToArray();
        result.Length.ShouldBe(source.Length - 10);

        // Take and TakeLast
        result = source.AsValueEnumerable().Shuffle().Take(15).TakeLast(7).ToArray();
        result.Length.ShouldBe(7);
    }

    [Fact]
    public void TryGetNonEnumeratedCount_Works()
    {
        var source = Enumerable.Range(1, 20).ToArray();

        // Simple Take
        var query = source.AsValueEnumerable().Shuffle().Take(7);
        query.TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(7);

        // Simple Skip
        query = source.AsValueEnumerable().Shuffle().Skip(7);
        query.TryGetNonEnumeratedCount(out count).ShouldBeTrue();
        count.ShouldBe(13);

        // Chained operations
        var query2 = source.AsValueEnumerable().Shuffle().Skip(3).Take(10);
        query2.TryGetNonEnumeratedCount(out count).ShouldBeTrue();
        count.ShouldBe(10);

        // Edge case: Skip more than available
        query = source.AsValueEnumerable().Shuffle().Skip(30);
        query.TryGetNonEnumeratedCount(out count).ShouldBeTrue();
        count.ShouldBe(0);

        // Edge case: Skip all then take
        query2 = source.AsValueEnumerable().Shuffle().Skip(20).Take(5);
        query2.TryGetNonEnumeratedCount(out count).ShouldBeTrue();
        count.ShouldBe(0);
    }

    [Fact]
    public void TryGetSpan_Works()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // Take
        var query = source.AsValueEnumerable().Shuffle().Take(5);
        query.TryGetSpan(out var span).ShouldBeTrue();
        span.Length.ShouldBe(5);

        // Skip
        query = source.AsValueEnumerable().Shuffle().Skip(3);
        query.TryGetSpan(out span).ShouldBeTrue();
        span.Length.ShouldBe(7);

        // Chained operations
        var query2 = source.AsValueEnumerable().Shuffle().Skip(2).Take(5);
        query2.TryGetSpan(out span).ShouldBeTrue();
        span.Length.ShouldBe(5);
    }

    [Fact]
    public void TryCopyTo_Works()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // Simple Take
        var query = source.AsValueEnumerable().Shuffle().Take(5);
        var destination = new int[5];
        query.TryCopyTo(destination).ShouldBeTrue();
        destination.Length.ShouldBe(5);

        // Destination small
        destination = new int[3];
        query.TryCopyTo(destination).ShouldBeTrue();

        // Partial copy with offset
        destination = new int[3];
        query.TryCopyTo(destination, 2).ShouldBeTrue();
    }

    [Fact]
    public void Enumeration_WithTryGetNext_Works()
    {
        var source = Enumerable.Range(1, 10).ToArray();

        // Take
        var enumerator = source.AsValueEnumerable().Shuffle().Take(5).Enumerator;
        var count = 0;
        while (enumerator.TryGetNext(out _))
        {
            count++;
        }
        count.ShouldBe(5);

        // Skip
        enumerator = source.AsValueEnumerable().Shuffle().Skip(7).Enumerator;
        count = 0;
        while (enumerator.TryGetNext(out _))
        {
            count++;
        }
        count.ShouldBe(3);
    }

    [Fact]
    public void Empty_Source_Handled_Correctly()
    {
        var empty = Array.Empty<int>();

        // Take
        var result = empty.AsValueEnumerable().Shuffle().Take(5).ToArray();
        result.Length.ShouldBe(0);

        // Skip
        result = empty.AsValueEnumerable().Shuffle().Skip(3).ToArray();
        result.Length.ShouldBe(0);

        // Chained operations
        result = empty.AsValueEnumerable().Shuffle().Skip(2).Take(3).ToArray();
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void ElementsPreserved_AfterShuffling()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        // All elements should be preserved after shuffle and operations
        var result = source.AsValueEnumerable().Shuffle().Take(50).ToArray();
        result.OrderBy(x => x).ToArray().ShouldNotBe(source.Take(50).ToArray());
        result.OrderBy(x => x).ToArray().ShouldBe(result.OrderBy(x => x).ToArray());

        // Skip should preserve remaining elements
        result = source.AsValueEnumerable().Shuffle().Skip(50).ToArray();
        result.Length.ShouldBe(50);
        result.OrderBy(x => x).ToArray().ShouldNotBe(source.Skip(50).ToArray());

        // Combined operations should preserve the right subset of elements
        result = source.AsValueEnumerable().Shuffle().Skip(25).Take(50).ToArray();
        result.Length.ShouldBe(50);
    }

    [Fact]
    public void Edge_Cases_IntMaxValue()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        // Skip 0, Take int.MaxValue
        var result = source.AsValueEnumerable().Shuffle().Skip(0).Take(int.MaxValue).ToArray();
        result.Length.ShouldBe(source.Length);

        // Skip int.MaxValue, Take anything
        result = source.AsValueEnumerable().Shuffle().Skip(int.MaxValue).Take(10).ToArray();
        result.Length.ShouldBe(0);

        // Chaining multiple skips that would exceed int.MaxValue
        var query = source.AsValueEnumerable().Shuffle().Skip(int.MaxValue - 10).Skip(20);
        query.TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(0);
    }
}
