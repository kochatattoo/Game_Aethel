#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZLinq.Tests.Linq;

public class InfiniteSequenceTests
{
    [Fact]
    public void NullArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>("start", () => ValueEnumerable.InfiniteSequence((ReferenceAddable)null!, new()));
        Assert.Throws<ArgumentNullException>("step", () => ValueEnumerable.InfiniteSequence(new(), (ReferenceAddable)null!));
    }

    [Fact]
    public void InfiniteSequence_AllZeroes_MatchesExpectedOutput()
    {
        Assert.Equal(Enumerable.Repeat(0, 10), ValueEnumerable.InfiniteSequence(0, 0).Take(10).ToArray());
        Assert.Equal(Enumerable.Repeat(0, 10).Select(i => (char)i), ValueEnumerable.InfiniteSequence((char)0, (char)0).Take(10).ToArray());
        Assert.Equal(Enumerable.Repeat(0, 10).Select(i => (BigInteger)i), ValueEnumerable.InfiniteSequence(BigInteger.Zero, BigInteger.Zero).Take(10).ToArray());
        Assert.Equal(Enumerable.Repeat(0, 10).Select(i => (float)i), ValueEnumerable.InfiniteSequence((float)0, 0).Take(10).ToArray());
    }

    [Fact]
    public void InfiniteSequence_ProducesExpectedSequence()
    {
        Validate<sbyte>(0, 1);
        Validate<sbyte>(sbyte.MaxValue - 3, 2);
        Validate<sbyte>(sbyte.MinValue, sbyte.MaxValue / 2);

        Validate<int>(0, 1);
        Validate<int>(4, -3);
        Validate<int>(int.MaxValue - 3, 2);
        Validate<int>(int.MinValue, int.MaxValue / 2);

        Validate<long>(0L, 1L);
        Validate<long>(-4L, -3L);
        Validate<long>(long.MaxValue - 3L, 2L);
        Validate<long>(long.MinValue, long.MaxValue / 2L);

        Validate<float>(0f, 1f);
        Validate<float>(0f, -1f);
        Validate<float>(float.MaxValue, 1f);
        Validate<float>(float.MinValue, float.MaxValue / 2f);

        Validate<BigInteger>(new BigInteger(long.MaxValue) * 3, (BigInteger)12345);
        Validate<BigInteger>(new BigInteger(long.MaxValue) * 3, (BigInteger)(-12345));

        void Validate<T>(T start, T step) where T : INumber<T>
        {
            var sequence = ValueEnumerable.InfiniteSequence(start, step);

            for (int trial = 0; trial < 2; trial++)
            {
                using var e = sequence.GetEnumerator();

                T expected = start;
                for (int i = 0; i < 10; i++)
                {
                    Assert.True(e.MoveNext());
                    Assert.Equal(expected, e.Current);

                    expected += step;
                }
            }
        }
    }

    private sealed class ReferenceAddable : IAdditionOperators<ReferenceAddable, ReferenceAddable, ReferenceAddable>
    {
        public static ReferenceAddable operator +(ReferenceAddable left, ReferenceAddable right) => left;
        public static ReferenceAddable operator checked +(ReferenceAddable left, ReferenceAddable right) => left;
    }
}

#endif
