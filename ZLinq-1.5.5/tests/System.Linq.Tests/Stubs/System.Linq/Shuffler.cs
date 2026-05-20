// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Linq.Tests
{
    public static class Shuffler
    {
#if NET10_0_OR_GREATER
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
            => ZLinq.Tests.Shuffler.Shuffle(source);
#else
        // Need to expose as extension methods.
        // Because Shuffle extension methods are used by order related tests.
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
            => ZLinq.Tests.Shuffler.Shuffle(source);
#endif
    }
}

namespace ZLinq.Tests
{
    public static class Shuffler
    {
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
        {
            var r = new Random(42);

            var array = source.ToArray();
            r.Shuffle(array);
            return array;
        }
    }
}
