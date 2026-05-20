// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace ZLinq.Tests
{
    public sealed class CreateOrderedEnumerableTests
    {
        [Fact(Skip = SkipReason.CreateOrderedEnumerable)]
        public void ThrowsNullKeySelector()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var enumerable = new int[0].Order();

                // ZLinq don't support CreateOrderedEnumerable. See: https://github.com/Cysharp/ZLinq/issues/88
                // enumerable.CreateOrderedEnumerable((Func<int, int>)null!, null, false);
            });
        }
    }
}
