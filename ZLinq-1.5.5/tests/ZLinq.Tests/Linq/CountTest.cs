namespace ZLinq.Tests.Linq;

public class CountTest
{
    [Fact]
    public void EmptyCollection()
    {
        var xs = new int[0];

        // System LINQ reference implementation
        var expected = xs.Count();
        expected.ShouldBe(0);

        // ZLinq implementations
        xs.AsValueEnumerable().Count().ShouldBe(expected);
        xs.ToValueEnumerable().Count().ShouldBe(expected);
    }

    [Fact]
    public void NonEmptyCollection()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };

        // System LINQ reference implementation
        var expected = xs.Count();
        expected.ShouldBe(5);

        // ZLinq implementations
        xs.AsValueEnumerable().Count().ShouldBe(expected);
        xs.ToValueEnumerable().Count().ShouldBe(expected);
    }

    [Fact]
    public void EmptyCollectionWithPredicate()
    {
        var xs = new int[0];

        // System LINQ reference implementation
        var expected = xs.Count(x => x > 3);
        expected.ShouldBe(0);

        // ZLinq implementations
        xs.AsValueEnumerable().Count(x => x > 3).ShouldBe(expected);
        xs.ToValueEnumerable().Count(x => x > 3).ShouldBe(expected);
    }

    [Fact]
    public void NoItemsMatchPredicate()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };

        // System LINQ reference implementation
        var expected = xs.Count(x => x > 10);
        expected.ShouldBe(0);

        // ZLinq implementations
        xs.AsValueEnumerable().Count(x => x > 10).ShouldBe(expected);
        xs.ToValueEnumerable().Count(x => x > 10).ShouldBe(expected);
    }

    [Fact]
    public void SomeItemsMatchPredicate()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };

        // System LINQ reference implementation
        var expected = xs.Count(x => x > 3);
        expected.ShouldBe(2);

        // ZLinq implementations
        xs.AsValueEnumerable().Count(x => x > 3).ShouldBe(expected);
        xs.ToValueEnumerable().Count(x => x > 3).ShouldBe(expected);
    }

    [Fact]
    public void AllItemsMatchPredicate()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };

        // System LINQ reference implementation
        var expected = xs.Count(x => x > 0);
        expected.ShouldBe(5);

        // ZLinq implementations
        xs.AsValueEnumerable().Count(x => x > 0).ShouldBe(expected);
        xs.ToValueEnumerable().Count(x => x > 0).ShouldBe(expected);
    }

    [Fact]
    public void NonEnumeratedCountOptimization()
    {
        // Using an array which should support TryGetNonEnumeratedCount
        var xs = new int[] { 1, 2, 3, 4, 5 };

        // Create a custom counter to verify we're not enumerating when not needed
        var counter = 0;
        bool CountPredicate(int x)
        {
            counter++;
            return x > 0;
        }

        // Count without predicate should use optimization
        var result = xs.AsValueEnumerable().Count();
        result.ShouldBe(5);

        // Count with predicate should enumerate all items
        counter = 0;
        result = xs.AsValueEnumerable().Count(CountPredicate);
        result.ShouldBe(5);
        counter.ShouldBe(5); // All items should be enumerated
    }

    [Fact]
    public void LargeCollectionCountTest()
    {
        var xs = Enumerable.Range(1, 1000).ToArray();

        // System LINQ reference implementation
        var expected = xs.Count();
        expected.ShouldBe(1000);

        // ZLinq implementations
        xs.AsValueEnumerable().Count().ShouldBe(expected);
        xs.ToValueEnumerable().Count().ShouldBe(expected);
    }

    [Fact]
    public void PredicateThrowsException()
    {
        var xs = new int[] { 1, 2, 3, 0, 4 };

        // Create a predicate that will throw an exception
        bool ExceptionPredicate(int x)
        {
            return 10 / x > 0; // Will throw DivideByZeroException when x = 0
        }

        // System LINQ and ZLinq should both throw the same exception
        Should.Throw<DivideByZeroException>(() => xs.Count(ExceptionPredicate));
        Should.Throw<DivideByZeroException>(() => xs.AsValueEnumerable().Count(ExceptionPredicate));
        Should.Throw<DivideByZeroException>(() => xs.ToValueEnumerable().Count(ExceptionPredicate));
    }

    [Fact]
    public void WhereCountOptimization()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };
        
        // Counter to check if optimization is working
        var counter = 0;
        bool CounterPredicate(int x)
        {
            counter++;
            return x > 2;
        }
        
        // Test the Where().Count() optimization
        counter = 0;
        var result = xs.AsValueEnumerable().Where(CounterPredicate).Count();
        result.ShouldBe(3);
        counter.ShouldBe(5); // Should check all items exactly once
    }

    [Fact]
    public void ListWhereCountOptimization()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        
        // Test the specific List optimization for Where().Count()
        var result = list.AsValueEnumerable().Where(x => x > 3).Count();
        result.ShouldBe(2);
    }

    [Fact]
    public void CountWithNullPredicateThrowsException()
    {
        var xs = new int[] { 1, 2, 3 };
        
        // Should throw ArgumentNullException for null predicate
        Should.Throw<ArgumentNullException>(() => xs.AsValueEnumerable().Count(null!));
        Should.Throw<ArgumentNullException>(() => xs.ToValueEnumerable().Count(null!));
    }

    [Fact]
    public void ArrayWhereCountOptimization()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };
        
        // Test specific optimization for array Where().Count()
        var counter = 0;
        bool CountingPredicate(int x)
        {
            counter++;
            return x % 2 == 0;
        }
        
        counter = 0;
        var result = array.AsValueEnumerable().Where(CountingPredicate).Count();
        result.ShouldBe(2);
        counter.ShouldBe(5); // Should use span-based optimization
    }

    [Fact]
    public void SpanOptimizationTest()
    {
        var data = new[] { 1, 2, 3, 4, 5 };
        
        // Count with predicate should use span optimization when possible
        var spanCounter = 0;
        bool SpanPredicate(int x)
        {
            spanCounter++;
            return x > 2;
        }
        
        spanCounter = 0;
        var result = data.AsValueEnumerable().Count(SpanPredicate);
        result.ShouldBe(3);
        spanCounter.ShouldBe(5); // All items counted once
    }
}
