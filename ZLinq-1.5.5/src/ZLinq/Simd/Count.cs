#if NET8_0_OR_GREATER

using System;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace ZLinq.Simd;

partial struct Vectorizable<T>
{
    public int Count(Func<Vector<T>, Vector<T>> vectorCompare, Func<T, bool> compare)
    {
        ReadOnlySpan<T> src = source;
        var count = 0;

        if (Vector256.IsHardwareAccelerated && Vector<T>.IsSupported && src.Length >= Vector<T>.Count && Vector<T>.Count >= Vector128<T>.Count)
        {
            var vectors = MemoryMarshal.Cast<T, Vector<T>>(src);

            if (Vector<T>.Count == Vector512<T>.Count)
            {
                foreach (var item in vectors)
                {
                    var compareResult = vectorCompare(item);
                    var bits = Vector512.ExtractMostSignificantBits(compareResult.AsVector512());
                    var popCount = BitOperations.PopCount(bits);
                    count += popCount;
                }
            }
            else if (Vector<T>.Count == Vector256<T>.Count)
            {
                foreach (var item in vectors)
                {
                    var compareResult = vectorCompare(item);
                    var bits = Vector256.ExtractMostSignificantBits(compareResult.AsVector256());
                    var popCount = BitOperations.PopCount(bits);
                    count += popCount;
                }
            }
            else if (Vector<T>.Count == Vector128<T>.Count)
            {
                foreach (var item in vectors)
                {
                    var compareResult = vectorCompare(item);
                    var bits = Vector128.ExtractMostSignificantBits(compareResult.AsVector128());
                    var popCount = BitOperations.PopCount(bits);
                    count += popCount;
                }
            }

            src = src.Slice(vectors.Length * Vector<T>.Count);
        }

        foreach (T item in src)
        {
            if (compare(item))
            {
                count++;
            }
        }

        return count;
    }
}

#endif
