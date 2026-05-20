namespace ZLinq.Tests.Linq;

public class ShuffleSkipTest
{
    [Fact]
    public void Shuffle_Skip()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        // Skip first 3 elements
        var result = source.AsValueEnumerable()
            .Shuffle()
            .Skip(3)
            .ToArray();
        result.Length.ShouldBe(source.Length - 3);

        // skip all
        result = source.AsValueEnumerable()
            .Shuffle()
            .Skip(int.MaxValue)
            .ToArray();
        result.Length.ShouldBe(0);

        // skip none
        result = source.AsValueEnumerable()
            .Shuffle()
            .Skip(int.MinValue)
            .ToArray();
        result.Length.ShouldBe(source.Length);
    }

    [Fact]
    public void Shuffle_SkipLast()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        // Skip first 3 elements
        var result = source.AsValueEnumerable()
            .Shuffle()
            .SkipLast(3)
            .ToArray();
        result.Length.ShouldBe(source.Length - 3);

        // skip all
        result = source.AsValueEnumerable()
            .Shuffle()
            .SkipLast(int.MaxValue)
            .ToArray();
        result.Length.ShouldBe(0);

        // skip none
        result = source.AsValueEnumerable()
            .Shuffle()
            .SkipLast(int.MinValue)
            .ToArray();
        result.Length.ShouldBe(source.Length);
    }

    [Fact]
    public void Shuffle_Take()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        // Take first 4 elements
        var result = source.AsValueEnumerable()
            .Shuffle()
            .Take(4)
            .ToArray();
        result.Length.ShouldBe(4);

        // take all
        result = source.AsValueEnumerable()
            .Shuffle()
            .Take(int.MaxValue)
            .ToArray();
        result.Length.ShouldBe(source.Length);

        // take none
        result = source.AsValueEnumerable()
            .Shuffle()
            .Take(int.MinValue)
            .ToArray();
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void Shuffle_TakeLast()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        // Take first 4 elements
        var result = source.AsValueEnumerable()
            .Shuffle()
            .TakeLast(4)
            .ToArray();
        result.Length.ShouldBe(4);

        // take all
        result = source.AsValueEnumerable()
            .Shuffle()
            .TakeLast(int.MaxValue)
            .ToArray();
        result.Length.ShouldBe(source.Length);

        // take none
        result = source.AsValueEnumerable()
            .Shuffle()
            .TakeLast(int.MinValue)
            .ToArray();
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void Shuffle_Skip_TryGetNonEnumeratedCount()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        var query = source.AsValueEnumerable()
            .Shuffle()
            .Skip(3);

        query.TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(6); // 9 - 3 = 6
    }

    [Fact]
    public void Shuffle_SkipLast_TryGetNonEnumeratedCount()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        var query = source.AsValueEnumerable()
            .Shuffle()
            .SkipLast(3);

        query.TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(6); // 9 - 3 = 6
    }

    [Fact]
    public void Shuffle_Take_TryGetNonEnumeratedCount()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        var query = source.AsValueEnumerable()
            .Shuffle()
            .Take(4);

        query.TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(4);
    }

    [Fact]
    public void Shuffle_TakeLast_TryGetNonEnumeratedCount()
    {
        var source = new[] { 5, 2, 8, 1, 9, 3, 7, 4, 6 };

        var query = source.AsValueEnumerable()
            .Shuffle()
            .TakeLast(4);

        query.TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(4);
    }

    [Fact]
    public void Shuffle()
    {
        // internally using ToArrayPool(). source size should not be power-of-2 for testing.
        var source = Enumerable.Range(0, 567).ToArray();

        List<int> list;
        list = source.AsValueEnumerable().Shuffle().TakeLast(int.MaxValue).ToList();
        Test(source, list);
        list = source.AsValueEnumerable().Shuffle().Take(int.MaxValue).ToList();
        Test(source, list);
        list = source.AsValueEnumerable().Shuffle().SkipLast(0).ToList();
        Test(source, list);
        list = source.AsValueEnumerable().Shuffle().Skip(0).ToList();
        Test(source, list);

        static void Test(int[] source, List<int> list)
        {
            list.Count.ShouldBe(source.Length);
            list.ToArray().ShouldNotBe(source);

            // no duplicates?
            foreach (var val in source)
            {
                list.Remove(val).ShouldBeTrue();
                list.IndexOf(val).ShouldBe(-1);
            }

            list.ShouldBeEmpty();
        }

        const int size = 100;

        // shuffle correctly takes items from all available source items?
        var items = source.Take(size).ToArray();
        source.AsValueEnumerable().Shuffle().Take(size).ToList().Except(items).ShouldNotBeEmpty();
        source.AsValueEnumerable().Shuffle().TakeLast(size).ToList().Except(items).ShouldNotBeEmpty();
        source.Except(source.AsValueEnumerable().Shuffle().Take(size).ToList()).Count().ShouldBe(source.Length - size);
        source.Except(source.AsValueEnumerable().Shuffle().TakeLast(size).ToList()).Count().ShouldBe(source.Length - size);

        items = source.Skip(size).ToArray();
        source.AsValueEnumerable().Shuffle().Skip(size).ToList().Except(items).ShouldNotBeEmpty();
        source.AsValueEnumerable().Shuffle().SkipLast(size).ToList().Except(items).ShouldNotBeEmpty();
        source.Except(source.AsValueEnumerable().Shuffle().Skip(size).ToList()).Count().ShouldBe(size);
        source.Except(source.AsValueEnumerable().Shuffle().SkipLast(size).ToList()).Count().ShouldBe(size);
    }

}
