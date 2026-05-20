#if NET8_0_OR_GREATER

using System;
using System.Numerics;

namespace ZLinq.Simd;

partial struct Vectorizable<T>
{
    public T Aggregate(Func<Vector<T>, Vector<T>, Vector<T>> vectorFunc, Func<T, T, T> func)
    {
        var src = source;

        if (src.Length == 0) Throws.NoElements<T>();

        T result;

        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && src.Length >= Vector<T>.Count)
        {
            var vectors = MemoryMarshal.Cast<T, Vector<T>>(src);

            var vectorResult = vectors[0];
            foreach (var item in vectors.Slice(1))
            {
                vectorResult = vectorFunc(vectorResult, item);
            }

            result = vectorResult[0];
            for (int i = 1; i < Vector<T>.Count; i++)
            {
                result = func(result, vectorResult[i]);
            }

            src = src.Slice(vectors.Length * Vector<T>.Count);
        }
        else
        {
            result = src[0];
            src = src.Slice(1);
        }

        foreach (T item in src)
        {
            result = func(result, item);
        }

        return result;
    }
}

#endif
