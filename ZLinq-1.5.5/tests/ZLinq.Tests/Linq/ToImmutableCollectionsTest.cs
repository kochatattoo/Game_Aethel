using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Shouldly;
using Xunit;

namespace ZLinq.Tests.Linq;

#if NET8_0_OR_GREATER

public class ToImmutableCollectionsTest
{
    #region ToImmutableArray Tests
    [Fact]
    public void ToImmutableArray_WithArray()
    {
        // Arrange
        int[] source = [1, 2, 3, 4, 5];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableArray();
        
        // Assert
        result.ShouldBeOfType<ImmutableArray<int>>();
        result.Length.ShouldBe(5);
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void ToImmutableArray_WithEmptyArray()
    {
        // Arrange
        int[] source = [];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableArray();
        
        // Assert
        result.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void ToImmutableArray_WithList()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c" };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableArray();
        
        // Assert
        result.ShouldBe(new[] { "a", "b", "c" });
    }
    
    [Fact]
    public void ToImmutableArray_WithSpanOptimization()
    {
        // Arrange - using a type that supports TryGetSpan optimization
        int[] source = [1, 2, 3, 4, 5];
        var valueEnumerable = source.AsValueEnumerable();
        
        // Verify span optimization is available
        valueEnumerable.TryGetSpan(out _).ShouldBeTrue();
        
        // Act
        var result = valueEnumerable.ToImmutableArray();
        
        // Assert
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }
    #endregion

    #region ToImmutableList Tests
    [Fact]
    public void ToImmutableList_WithArray()
    {
        // Arrange
        int[] source = [1, 2, 3, 4, 5];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableList();
        
        // Assert
        result.ShouldBeOfType<ImmutableList<int>>();
        result.Count.ShouldBe(5);
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void ToImmutableList_WithEmptyArray()
    {
        // Arrange
        int[] source = [];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableList();
        
        // Assert
        result.Count.ShouldBe(0);
    }
    
    [Fact]
    public void ToImmutableList_WithSpanOptimization()
    {
        // Arrange - using a type that supports TryGetSpan optimization
        int[] source = [1, 2, 3, 4, 5];
        var valueEnumerable = source.AsValueEnumerable();
        
        // Verify span optimization is available
        valueEnumerable.TryGetSpan(out _).ShouldBeTrue();
        
        // Act
        var result = valueEnumerable.ToImmutableList();
        
        // Assert
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }
    #endregion

    #region ToImmutableDictionary Tests
    [Fact]
    public void ToImmutableDictionary_WithKeySelector()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableDictionary(x => x.ToString());
        
        // Assert
        result.ShouldBeOfType<ImmutableDictionary<string, int>>();
        result.Count.ShouldBe(5);
        result["1"].ShouldBe(1);
        result["5"].ShouldBe(5);
    }

    [Fact]
    public void ToImmutableDictionary_WithKeyAndElementSelectors()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableDictionary(
            x => x.ToString(), 
            x => x * 10
        );
        
        // Assert
        result.ShouldBeOfType<ImmutableDictionary<string, int>>();
        result.Count.ShouldBe(5);
        result["1"].ShouldBe(10);
        result["5"].ShouldBe(50);
    }
    
    [Fact]
    public void ToImmutableDictionary_WithCustomComparer()
    {
        // Arrange
        var source = new[] { "a", "A", "b", "B" };
        var comparer = StringComparer.OrdinalIgnoreCase;
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableDictionary(
            x => x, 
            x => x.ToUpperInvariant(),
            comparer
        );
        
        // Assert
        result.Count.ShouldBe(2); // Case-insensitive, so only 2 unique keys
        result["a"].ShouldBe("A");
        result["b"].ShouldBe("B");
    }
    
    [Fact]
    public void ToImmutableDictionary_FromKeyValuePairs()
    {
        // Arrange
        var source = new[]
        {
            new KeyValuePair<string, int>("one", 1),
            new KeyValuePair<string, int>("two", 2),
            new KeyValuePair<string, int>("three", 3)
        };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableDictionary();
        
        // Assert
        result.Count.ShouldBe(3);
        result["one"].ShouldBe(1);
        result["three"].ShouldBe(3);
    }
    #endregion

    #region ToImmutableSortedDictionary Tests
    [Fact]
    public void ToImmutableSortedDictionary_WithKeySelector()
    {
        // Arrange
        var source = new[] { 5, 3, 1, 4, 2 };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedDictionary(x => x.ToString());
        
        // Assert
        result.ShouldBeOfType<ImmutableSortedDictionary<string, int>>();
        result.Count.ShouldBe(5);
        
        // Verify it's sorted by keys
        result.Keys.ShouldBe(new[] { "1", "2", "3", "4", "5" });
    }

    [Fact]
    public void ToImmutableSortedDictionary_WithKeyAndElementSelectors()
    {
        // Arrange
        var source = new[] { 5, 3, 1, 4, 2 };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedDictionary(
            x => x.ToString(), 
            x => x * 10
        );
        
        // Assert
        result.ShouldBeOfType<ImmutableSortedDictionary<string, int>>();
        result.Count.ShouldBe(5);
        
        // Verify it's sorted by keys
        result.Keys.ShouldBe(new[] { "1", "2", "3", "4", "5" });
        result["1"].ShouldBe(10);
        result["5"].ShouldBe(50);
    }
    
    [Fact]
    public void ToImmutableSortedDictionary_WithCustomComparer()
    {
        // Arrange
        var source = new[] { "aa", "BB", "c", "DD" };
        var comparer = StringComparer.OrdinalIgnoreCase; // Case-insensitive comparer
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedDictionary(
            x => x, 
            x => x.Length,
            comparer
        );
        
        // Assert
        result.Count.ShouldBe(4);
        
        // Should be sorted by keys (case-insensitive)
        var keys = result.Keys.ToArray();
        Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
        result.Keys.ShouldBe(keys);
    }
    
    [Fact]
    public void ToImmutableSortedDictionary_FromKeyValuePairs()
    {
        // Arrange
        var source = new[]
        {
            new KeyValuePair<string, int>("c", 3),
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 2)
        };
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedDictionary();
        
        // Assert
        result.Count.ShouldBe(3);
        result.Keys.ShouldBe(new[] { "a", "b", "c" }); // Sorted keys
    }
    #endregion

    #region ToImmutableHashSet Tests
    [Fact]
    public void ToImmutableHashSet_WithArray()
    {
        // Arrange
        int[] source = [1, 2, 3, 3, 4, 5, 5];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableHashSet();
        
        // Assert
        result.ShouldBeOfType<ImmutableHashSet<int>>();
        result.Count.ShouldBe(5); // Duplicates are removed
        result.Contains(1).ShouldBeTrue();
        result.Contains(3).ShouldBeTrue();
        result.Contains(6).ShouldBeFalse();
    }

    [Fact]
    public void ToImmutableHashSet_WithEmptyArray()
    {
        // Arrange
        int[] source = [];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableHashSet();
        
        // Assert
        result.Count.ShouldBe(0);
    }
    
    [Fact]
    public void ToImmutableHashSet_WithCustomEqualityComparer()
    {
        // Arrange
        var source = new[] { "a", "A", "b", "B", "c" };
        var comparer = StringComparer.OrdinalIgnoreCase;
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableHashSet(comparer);
        
        // Assert
        result.Count.ShouldBe(3); // Case-insensitive, so only 3 unique values
        result.Contains("a").ShouldBeTrue();
        result.Contains("A").ShouldBeTrue(); // Both "a" and "A" match the same element
    }
    
    [Fact]
    public void ToImmutableHashSet_WithSpanOptimization()
    {
        // Arrange - using a type that supports TryGetSpan optimization
        int[] source = [1, 2, 3, 4, 5];
        var valueEnumerable = source.AsValueEnumerable();
        
        // Verify span optimization is available
        valueEnumerable.TryGetSpan(out _).ShouldBeTrue();
        
        // Act
        var result = valueEnumerable.ToImmutableHashSet();
        
        // Assert
        result.Count.ShouldBe(5);
        result.Contains(3).ShouldBeTrue();
    }
    #endregion

    #region ToImmutableSortedSet Tests
    [Fact]
    public void ToImmutableSortedSet_WithArray()
    {
        // Arrange
        int[] source = [5, 3, 1, 4, 2, 3, 5];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedSet();
        
        // Assert
        result.ShouldBeOfType<ImmutableSortedSet<int>>();
        result.Count.ShouldBe(5); // Duplicates are removed
        
        // Verify it's sorted
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void ToImmutableSortedSet_WithEmptyArray()
    {
        // Arrange
        int[] source = [];
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedSet();
        
        // Assert
        result.Count.ShouldBe(0);
    }
    
    [Fact]
    public void ToImmutableSortedSet_WithCustomComparer()
    {
        // Arrange
        var source = new[] { "aa", "BB", "c", "DD" };
        var comparer = StringComparer.OrdinalIgnoreCase; // Case-insensitive comparer
        
        // Act
        var result = source.AsValueEnumerable().ToImmutableSortedSet(comparer);
        
        // Assert
        result.Count.ShouldBe(4);
        
        // Should be sorted by the comparer
        var expected = new[] { "aa", "BB", "c", "DD" };
        Array.Sort(expected, comparer);
        result.ShouldBe(expected);
    }
    
    [Fact]
    public void ToImmutableSortedSet_WithSpanOptimization()
    {
        // Arrange - using a type that supports TryGetSpan optimization
        int[] source = [5, 3, 1, 4, 2];
        var valueEnumerable = source.AsValueEnumerable();
        
        // Verify span optimization is available
        valueEnumerable.TryGetSpan(out _).ShouldBeTrue();
        
        // Act
        var result = valueEnumerable.ToImmutableSortedSet();
        
        // Assert
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 }); // Sorted order
    }
    #endregion
}

#endif
