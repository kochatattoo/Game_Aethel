using System.Runtime.CompilerServices;
using ZLinq;
using ZLinq.Linq;

namespace ZLinq.Tests;

public static partial class Assert
{
    internal static void Empty<TEnumerator, T>(ValueEnumerable<TEnumerator, T> value)
        where TEnumerator : struct, IValueEnumerator<T>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        => Assert.Empty(value.ToArray());

    internal static void Empty(ValueEnumerable<Cast<FromEnumerable<object>, object, int>, int> collection)
        => Xunit.Assert.Empty(collection.ToArray());
}
