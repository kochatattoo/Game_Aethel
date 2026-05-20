namespace ZLinq.Tests;

public static partial class Assert
{
    internal static void DoesNotContain<TEnumerator, T>(
        T expected,
        ValueEnumerable<TEnumerator, T> valueEnumerable)
        where TEnumerator : struct, IValueEnumerator<T>
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
     => Xunit.Assert.DoesNotContain(expected, valueEnumerable.ToArray());
}
