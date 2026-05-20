namespace ZLinq.Tests.Linq;

public class Empty
{
    [Fact]
    public void Standard()
    {
        ValueEnumerable.Empty<int>().Select(x => x).ToArray().ShouldBe(Enumerable.Empty<int>().ToArray());

        ValueEnumerable.Empty<int>().TryGetNonEnumeratedCount(out var count).ShouldBeTrue();
        count.ShouldBe(0);
    }

    [Fact]
    public void ForEach()
    {
        var e = Enumerable.Empty<int>().GetEnumerator();
        foreach (var item in ValueEnumerable.Empty<int>())
        {
            e.MoveNext();
            item.ShouldBe(e.Current);
        }
    }

    [Fact]
    public void TryGetSpanTryCopyTo()
    {
        var valueEnumerable = ValueEnumerable.Empty<int>();

        valueEnumerable.TryGetSpan(out var span).ShouldBeTrue();
        span.Length.ShouldBe(0);

        valueEnumerable.TryCopyTo([], 0).ShouldBeTrue();
    }
}
