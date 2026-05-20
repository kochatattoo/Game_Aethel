namespace ZLinq.Tests.Linq;

public class RangeDateTimeTest
{
    [Fact]
    public void DateTimeRangeBasicTest()
    {
        // Test with DateTime and step
        var start = new DateTime(2023, 1, 1);
        var step = TimeSpan.FromDays(1);
        var range = ValueEnumerable.Range(start, 5, step).ToArray();
        
        range.Length.ShouldBe(5);
        for (int i = 0; i < 5; i++)
        {
            range[i].ShouldBe(start.AddDays(i));
        }
    }
    
    [Fact]
    public void DateTimeRangeToTest()
    {
        // Test DateTime Range with end value
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 10);
        var step = TimeSpan.FromDays(2);
        
        // Exclusive end bound (default)
        var rangeExclusive = ValueEnumerable.Range(start, end, step, RightBound.Exclusive).ToArray();
        rangeExclusive.Length.ShouldBe(5); // Jan 1, 3, 5, 7, 9 (5 days)
        rangeExclusive[0].ShouldBe(start);
        rangeExclusive[^1].ShouldBe(new DateTime(2023, 1, 9));
        
        // Inclusive end bound
        var rangeInclusive = ValueEnumerable.Range(start, end, step, RightBound.Inclusive).ToArray();
        rangeInclusive.Length.ShouldBe(5); // Jan 1, 3, 5, 7, 9 (5 days) - inclusive doesn't change because step pushes past the boundary
        
        // Test with end date exactly matching the step interval
        var exactEnd = new DateTime(2023, 1, 9);
        var rangeExact = ValueEnumerable.Range(start, exactEnd, step, RightBound.Inclusive).ToArray();
        rangeExact.Length.ShouldBe(5); // Jan 1, 3, 5, 7, 9 (5 days)
        rangeExact[^1].ShouldBe(exactEnd);
    }
    
    [Fact]
    public void DateTimeOffsetRangeTest()
    {
        // Test DateTimeOffset with count and step
        var start = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.FromHours(2));
        var step = TimeSpan.FromHours(12);
        var range = ValueEnumerable.Range(start, 5, step).ToArray();
        
        range.Length.ShouldBe(5);
        for (int i = 0; i < 5; i++)
        {
            range[i].ShouldBe(start.AddHours(12 * i));
        }
    }
    
    [Fact]
    public void DateTimeOffsetRangeToTest()
    {
        // Test DateTimeOffset with end boundary
        var start = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2023, 1, 3, 0, 0, 0, TimeSpan.Zero);
        var step = TimeSpan.FromHours(12);
        
        // Exclusive boundary
        var rangeExclusive = ValueEnumerable.Range(start, end, step, RightBound.Exclusive).ToArray();
        rangeExclusive.ShouldBe(new[] {
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 2, 12, 0, 0, TimeSpan.Zero)
        });
        
        // Inclusive boundary
        var rangeInclusive = ValueEnumerable.Range(start, end, step, RightBound.Inclusive).ToArray();
        rangeInclusive.ShouldBe(new[] {
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 2, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 3, 0, 0, 0, TimeSpan.Zero)
        });
    }
    
    [Fact]
    public void DateTimeSmallTimeSpanTest()
    {
        // Test with small time increments
        var start = new DateTime(2023, 1, 1);
        var step = TimeSpan.FromMilliseconds(100);
        var range = ValueEnumerable.Range(start, 10, step).ToArray();
        
        range.Length.ShouldBe(10);
        for (int i = 0; i < 10; i++)
        {
            range[i].ShouldBe(start.AddMilliseconds(i * 100));
        }
    }
    
    [Fact]
    public void EdgeCasesTest()
    {
        // Test with DateTime.MinValue
        var minStart = DateTime.MinValue;
        var step = TimeSpan.FromDays(1);
        var rangeFromMin = ValueEnumerable.Range(minStart, 3, step).ToArray();
        rangeFromMin.ShouldBe(new[] {
            DateTime.MinValue,
            DateTime.MinValue.AddDays(1),
            DateTime.MinValue.AddDays(2)
        });
        
        // Test with DateTime.MaxValue (approaching limit)
        var nearMax = DateTime.MaxValue.AddDays(-3);
        var rangeNearMax = ValueEnumerable.Range(nearMax, 3, step).ToArray();
        rangeNearMax.ShouldBe(new[] {
            nearMax,
            nearMax.AddDays(1),
            nearMax.AddDays(2)
        });
        
        // Test empty range
        var emptyRange = ValueEnumerable.Range(minStart, 0, step).ToArray();
        emptyRange.ShouldBeEmpty();
    }
}
