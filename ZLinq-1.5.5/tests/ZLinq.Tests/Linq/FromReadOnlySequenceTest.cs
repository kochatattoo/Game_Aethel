using System;
using System.Buffers;
using System.Collections.Immutable;

namespace ZLinq.Tests.Linq;

#if NET6_0_OR_GREATER

#pragma warning disable CS8500 // CS8500: This takes the address of, gets the size of, or declares a pointer to a managed type 

public class FromReadOnlySequenceTest
{
    [Fact]
    public void TryGetNext()
    {
        var segment1 = new TestReadOnlySegment<int>(new int[] { 1, 2, 3, 4, 5 });
        var segment2 = segment1.Append(new int[] { 6, 7, 8, 9, 10 });
        var source = new ReadOnlySequence<int>(segment1, 0, segment2, 5);
        var valueEnumerable = source.AsValueEnumerable();

        foreach (var i in valueEnumerable)
        {
        }
    }

    [Fact]
    public void TryGetSpan()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>([1, 2, 3, 4, 5]);
        var valueEnumerable = source.AsValueEnumerable();
        using var e = valueEnumerable.Enumerator;
        e.TryGetSpan(out var span).ShouldBeTrue();

        IsSameSpan(source.FirstSpan, span).ShouldBeTrue();

        // Additional tests
#pragma warning disable IDE0300 // Simplify collection initialization
        IsSameSpan(source.FirstSpan, new[] { 1, 2, 3, 4, 5 }.AsSpan()).ShouldBeFalse(); // AsSpan() is required. Because without AsSpan call. it seems to be optimized.
#pragma warning restore IDE0300
    }

    [Fact]
    public void TryGetSpan_MultiSegment()
    {
        var segment1 = new TestReadOnlySegment<int>(new int[] { 1, 2, 3, 4, 5 });
        var segment2 = segment1.Append(new int[] { 6, 7, 8, 9, 10 });
        var source = new ReadOnlySequence<int>(segment1, 0, segment2, 5);
        var valueEnumerable = source.AsValueEnumerable();

        source.IsSingleSegment.ShouldBeFalse();
        valueEnumerable.TryGetSpan(out var span).ShouldBeFalse();
    }

    [Fact]
    public void TryGetNext_EmptyImmutableArray()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>([]);
        var valueEnumerable = source.AsValueEnumerable();

        valueEnumerable.TryGetSpan(out var span).ShouldBeTrue();

        IsSameSpan(source.FirstSpan, span).ShouldBeTrue();
        IsSameSpan(span, ImmutableArray<int>.Empty.AsSpan()).ShouldBeTrue();
    }

    [Fact]
    public void TryGetNonEnumeratedCount()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>([1, 2, 3, 4, 5]);
        var valueEnumerable = source.AsValueEnumerable();

        // Test getting count without enumeration
        valueEnumerable.TryGetNonEnumeratedCount(out var count1).ShouldBeTrue();
        ((long)count1).ShouldBe(source.Length);
    }

    [Fact(Skip = "Requires CPU/Memory resources to execute.")]
    public void TryGetNonEnumeratedCount_Over_ArrayMaxLength()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>(Enumerable.Range(1, int.MaxValue).ToArray());
        var valueEnumerable = source.AsValueEnumerable();

        valueEnumerable.TryGetNonEnumeratedCount(out var count).ShouldBeFalse();
        count.ShouldBe(0);
    }

    [Fact]
    public void AsValueEnumerable()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>([1, 2, 3, 4, 5]);
        var valueEnumerable = source.AsValueEnumerable();

        // Test getting count without enumeration
        valueEnumerable.TryGetNonEnumeratedCount(out var count1).ShouldBeTrue();
        ((long)count1).ShouldBe(source.Length);
    }

    [Fact]
    public void TryCopyTo_SingleSegment()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>([1, 2, 3, 4, 5]);
        var valueEnumerable = source.AsValueEnumerable();

        var destination = new int[5];
        valueEnumerable.TryCopyTo(destination, 0).ShouldBeTrue();
        destination.ShouldBe([1, 2, 3, 4, 5]);
        source.IsSingleSegment.ShouldBeTrue();
    }

    [Fact]
    public void TryCopyTo_MultiSegment()
    {
        var segment1 = new TestReadOnlySegment<int>(new int[] { 1, 2, 3, 4, 5 });
        var segment2 = segment1.Append(new int[] { 6, 7, 8, 9, 10 });
        var source = new ReadOnlySequence<int>(segment1, 0, segment2, 5);
        var valueEnumerable = source.AsValueEnumerable();

        var destination = new int[10];
        valueEnumerable.TryCopyTo(destination).ShouldBeTrue();
        destination.ShouldBe([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        source.IsSingleSegment.ShouldBeFalse();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        ReadOnlySequence<int> source = new ReadOnlySequence<int>([1, 2, 3, 4, 5]);
        var valueEnumerable = source.AsValueEnumerable();

        // Should not throw exceptions
        valueEnumerable.Dispose();
        valueEnumerable.Dispose();
        valueEnumerable.Dispose();
    }

    private static unsafe bool IsSameSpan<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual)
        where T : unmanaged
    {
        var ptrExpected = Unsafe.AsPointer(ref Unsafe.AsRef(in expected.GetPinnableReference()));
        var ptrActual = Unsafe.AsPointer(ref Unsafe.AsRef(in actual.GetPinnableReference()));

        return ptrExpected == ptrActual && expected.Length == actual.Length;
    }

    private class TestReadOnlySegment<T> : ReadOnlySequenceSegment<T>
    {
        public TestReadOnlySegment(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public TestReadOnlySegment<T> Append(ReadOnlyMemory<T> memory)
        {
            var segment = new TestReadOnlySegment<T>(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;
            return segment;
        }
    }
}
#endif
