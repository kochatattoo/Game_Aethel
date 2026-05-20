using System;

namespace ZLinq.Tests.Linq;

#if NET9_0_OR_GREATER

#pragma warning disable CS8500 // CS8500: This takes the address of, gets the size of, or declares a pointer to a managed type 

public class FromSpanTest
{
    [Fact]
    public void TryGetNext()
    {
        ReadOnlySpan<int> source = [1, 2, 3, 4, 5];
        var valueEnumerable = source.AsValueEnumerable();

        foreach (var i in valueEnumerable)
        {
        }
    }

    [Fact]
    public void TryGetSpan()
    {
        ReadOnlySpan<int> source = [1, 2, 3, 4, 5];
        var valueEnumerable = source.AsValueEnumerable();
        using var e = valueEnumerable.Enumerator;
        e.TryGetSpan(out var span).ShouldBeTrue();

        IsSameSpan(source, span).ShouldBeTrue();

        // Additional tests

        IsSameSpan(source, [1, 2, 3, 4, 5]).ShouldBeTrue();

#pragma warning disable IDE0300 // Simplify collection initialization
        IsSameSpan(source, new[] { 1, 2, 3, 4, 5 }.AsSpan()).ShouldBeFalse(); // AsSpan() is required. Because without AsSpan call. it seems to be optimized.
#pragma warning restore IDE0300
    }

    [Fact]
    public void TryGetNext_EmptySpan()
    {
        ReadOnlySpan<int> source = [];
        var valueEnumerable = source.AsValueEnumerable();

        valueEnumerable.TryGetSpan(out var span).ShouldBeTrue();

        IsSameSpan(source, span).ShouldBeTrue();
    }

    [Fact]
    public void TryGetNonEnumeratedCount()
    {
        ReadOnlySpan<int> source = [1, 2, 3, 4, 5];
        var valueEnumerable = source.AsValueEnumerable();

        // Test getting count without enumeration
        valueEnumerable.TryGetNonEnumeratedCount(out var count1).ShouldBeTrue();
        count1.ShouldBe(source.Length);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        ReadOnlySpan<int> source = [1, 2, 3, 4, 5];
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
}
#endif
