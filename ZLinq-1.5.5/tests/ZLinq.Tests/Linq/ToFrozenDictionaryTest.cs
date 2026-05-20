namespace ZLinq.Tests.Linq;

#if NET8_0_OR_GREATER

public class ToFrozenDictionaryTest
{
    // Pattern follows existing test files:
    // Span = Array
    // EnumeratedCount Only = Select
    // Iterator = ToValueEnumerable

    [Fact]
    public void Empty()
    {
        var xs = new int[0];
        {
            // KeyValuePair source
            var kvpSource = xs.AsValueEnumerable().Select(x => new KeyValuePair<int, int>(x, x));
            var actual1 = kvpSource.ToFrozenDictionary();
            
            actual1.Count.ShouldBe(0);
        }
        {
            // With key selector
            var actual1 = xs.AsValueEnumerable().ToFrozenDictionary(x => x);
            var actual2 = xs.AsValueEnumerable().Select(x => x).ToFrozenDictionary(x => x);
            var actual3 = xs.ToValueEnumerable().ToFrozenDictionary(x => x);

            actual1.Count.ShouldBe(0);
            actual2.Count.ShouldBe(0);
            actual3.Count.ShouldBe(0);
        }
        {
            // With key and element selectors
            var actual1 = xs.AsValueEnumerable().ToFrozenDictionary(x => x, x => x);
            var actual2 = xs.AsValueEnumerable().Select(x => x).ToFrozenDictionary(x => x, x => x);
            var actual3 = xs.ToValueEnumerable().ToFrozenDictionary(x => x, x => x);

            actual1.Count.ShouldBe(0);
            actual2.Count.ShouldBe(0);
            actual3.Count.ShouldBe(0);
        }
    }

    [Fact]
    public void NonEmpty()
    {
        var xs = new int[] { 1, 2, 3, 4, 5 };
        {
            // KeyValuePair source
            var kvpSource = xs.AsValueEnumerable().Select(x => new KeyValuePair<int, int>(x, x * x));
            var actual1 = kvpSource.ToFrozenDictionary();
            
            Assert.Equal(xs.Length, actual1.Count);
            foreach (var x in xs)
            {
                Assert.Equal(x * x, actual1[x]);
            }
        }
        {
            // With key selector
            var actual1 = xs.AsValueEnumerable().ToFrozenDictionary(x => x);
            var actual2 = xs.AsValueEnumerable().Select(x => x).ToFrozenDictionary(x => x);
            var actual3 = xs.ToValueEnumerable().ToFrozenDictionary(x => x);
            
            Assert.Equal(xs.Length, actual1.Count);
            Assert.Equal(xs.Length, actual2.Count);
            Assert.Equal(xs.Length, actual3.Count);
            
            foreach (var x in xs)
            {
                Assert.Equal(x, actual1[x]);
                Assert.Equal(x, actual2[x]);
                Assert.Equal(x, actual3[x]);
            }
        }
        {
            // With key and element selectors
            var actual1 = xs.AsValueEnumerable().ToFrozenDictionary(x => x, x => x * x);
            var actual2 = xs.AsValueEnumerable().Select(x => x).ToFrozenDictionary(x => x, x => x * x);
            var actual3 = xs.ToValueEnumerable().ToFrozenDictionary(x => x, x => x * x);
            
            Assert.Equal(xs.Length, actual1.Count);
            Assert.Equal(xs.Length, actual2.Count);
            Assert.Equal(xs.Length, actual3.Count);
            
            foreach (var x in xs)
            {
                Assert.Equal(x * x, actual1[x]);
                Assert.Equal(x * x, actual2[x]);
                Assert.Equal(x * x, actual3[x]);
            }
        }
    }

    [Fact]
    public void Comparer()
    {
        (string, int)[] xs = [("foo", 100), ("bar", 200), ("baz", 300)];

        {
            // KeyValuePair source with comparer
            var kvpSource = xs.AsValueEnumerable().Select(x => new KeyValuePair<string, int>(x.Item1, x.Item2));
            var actual = kvpSource.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            
            actual.Count.ShouldBe(3);
            actual["FOO"].ShouldBe(100);
            actual["BAR"].ShouldBe(200);
            actual["BAZ"].ShouldBe(300);
        }
        {
            // With key selector and comparer
            var actual = xs.AsValueEnumerable().ToFrozenDictionary(x => x.Item1, StringComparer.OrdinalIgnoreCase);
            
            actual.Count.ShouldBe(3);
            actual["FOO"].ShouldBe(("foo", 100));
            actual["BAR"].ShouldBe(("bar", 200));
            actual["BAZ"].ShouldBe(("baz", 300));
        }
        {
            // With key and element selectors and comparer
            var actual = xs.AsValueEnumerable().ToFrozenDictionary(x => x.Item1, x => x.Item2, StringComparer.OrdinalIgnoreCase);
            
            actual.Count.ShouldBe(3);
            actual["FOO"].ShouldBe(100);
            actual["BAR"].ShouldBe(200);
            actual["BAZ"].ShouldBe(300);
        }
    }

    [Fact]
    public void DuplicateKeys()
    {
        var xs = new (int Key, int Value)[] { (1, 10), (2, 20), (1, 30) };

        // KeyValuePair source with duplicate keys should use last value seen
        var kvpSource = xs.AsValueEnumerable().Select(x => new KeyValuePair<int, int>(x.Key, x.Value));
        var dict = kvpSource.ToFrozenDictionary();
        
        dict.Count.ShouldBe(2);
        dict[1].ShouldBe(30);
        dict[2].ShouldBe(20);
    }
}

#endif
