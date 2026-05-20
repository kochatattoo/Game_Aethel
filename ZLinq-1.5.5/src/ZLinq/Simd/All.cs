#if NET8_0_OR_GREATER

using System;
using System.Numerics;

namespace ZLinq.Simd;

partial struct Vectorizable<T>
{
    public bool All(Func<Vector<T>, bool> vectorPredicate, Func<T, bool> predicate)
    {
        var src = source;

        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && src.Length >= Vector<T>.Count)
        {
            var vectors = MemoryMarshal.Cast<T, Vector<T>>(src);
            foreach (var item in vectors)
            {
                if (!vectorPredicate(item))
                {
                    return false;
                }
            }

            src = src.Slice(vectors.Length * Vector<T>.Count);
        }

        foreach (T item in src)
        {
            if (!predicate(item))
            {
                return false;
            }
        }

        return true;
    }
}

#endif
