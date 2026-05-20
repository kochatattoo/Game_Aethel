using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLinq;

namespace FileGen;

public partial class Commands
{
    readonly static string[] PrimitiveTypes = ["byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "bool", "char", "decimal", "nint", "nuint"];
    readonly static string[] PrimitiveTypesPlusString = [.. PrimitiveTypes, "string"];
    readonly static string[] PrimitiveNumbers = ["byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal", "nint", "nuint"];
    readonly static string[] PrimitiveNumbersWithoutFloat = ["byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "double", "decimal", "nint", "nuint"];
    readonly static string[] PrimitivesForMinMax = ["byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "nint", "nuint", "Int128", "UInt128"];

    readonly static string[] PrimitivesForSeqeunceOptimizable = ["Byte", "SByte", "UInt16", "Int16", "UInt32", "Int32", "UInt64", "Int64", "Char"]; // , "UIntPtr", "IntPtr"];
    readonly static string[] PrimitivesForSeqeunceOthers = ["Single", "Double", "Decimal"]; //, "DateTime", "DateTimeOffset"];

    public void TypeOfContains()
    {
        var sb = new StringBuilder();
        foreach (var type in PrimitiveTypesPlusString)
        {
            var code = $$"""
            else if (typeof(T) == typeof({{type}}))
            {
                var span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, {{type}}> (ref MemoryMarshal.GetReference(source)), source.Length);
                return MemoryExtensions.Contains(span, Unsafe.As<T, {{type}}>(ref value));
            }
""";
            sb.AppendLine(code);
        }

        Console.WriteLine(sb.ToString());
    }

    public void Sum()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#region generate from FileGen.Commands.Sum");
        foreach (var type in PrimitiveNumbersWithoutFloat)
        {
            var code = $$"""
        else if (typeof(TSource) == typeof({{type}}))
        {
            using (var enumerator = source.Enumerator)
            {
                {{type}} sum = default;
                while (enumerator.TryGetNext(out var item))
                {
                    checked { sum += Unsafe.As<TSource, {{type}}>(ref item); }
                }
                return Unsafe.As<{{type}}, TSource>(ref sum);
            }
        }
""";
            sb.AppendLine(code);
        }
        sb.AppendLine("#endregion");

        Console.WriteLine(sb.ToString());
    }

    public void Average()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#region generate from FileGen.Commands.Average");
        foreach (var type in PrimitiveNumbersWithoutFloat)
        {
            var code = $$"""
        else if (typeof(TSource) == typeof({{type}}))
        {
            using (var enumerator = source.Enumerator)
            {
                if (!enumerator.TryGetNext(out var current))
                {
                    Throws.NoElements();
                }

                {{type}} sum = Unsafe.As<TSource, {{type}}>(ref current);
                long count = 1;
                while (enumerator.TryGetNext(out current))
                {
                    checked { sum += Unsafe.As<TSource, {{type}}>(ref current); }
                    count++;
                }

                return (double)sum / (double)count;
            }
        }
""";
            sb.AppendLine(code);
        }
        sb.AppendLine("#endregion");

        Console.WriteLine(sb.ToString());
    }

    public void AverageNullable()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#region generate from FileGen.Commands.AverageNullable");
        foreach (var type in PrimitiveNumbersWithoutFloat)
        {
            var code = $$"""
        else if (typeof(TSource) == typeof({{type}}))
        {
            using (var enumerator = source.Enumerator)
            {
                while (enumerator.TryGetNext(out var first))
                {
                    if (first.HasValue)
                    {
                        var firstValue = first.GetValueOrDefault();
                        var sum = Unsafe.As<TSource, {{type}}>(ref firstValue);
                        long count = 1;

                        while (enumerator.TryGetNext(out var current))
                        {
                            if (current.HasValue)
                            {
                                var currentValue = current.GetValueOrDefault();
                                checked { sum += Unsafe.As<TSource, {{type}}>(ref currentValue); }
                                count++;
                            }
                        }

                        return (double)sum / (double)count;
                    }
                }
                return null;
            }
        }
""";
            sb.AppendLine(code);
        }
        sb.AppendLine("#endregion");

        Console.WriteLine(sb.ToString());
    }

    public void Min()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#region generate from FileGen.Commands.Min");
        foreach (var type in PrimitivesForMinMax)
        {
            var code = $$"""
        else if (typeof(TSource) == typeof({{type}}))
        {
            if (comparer != Comparer<TSource>.Default) return MinSpanComparer(span, comparer);
            var result = SimdMinBinaryInteger(UnsafeSpanBitCast<TSource, {{type}}>(span));
            return Unsafe.As<{{type}}, TSource>(ref result);
        }
""";
            sb.AppendLine(code);
        }
        sb.AppendLine("#endregion");

        Console.WriteLine(sb.ToString());
    }

    public void InterpolatedStringHandlerAppendFormatted()
    {
        var sb = new StringBuilder();
        foreach (var type in PrimitiveTypes)
        {
            var code = $$"""
        else if (typeof(T) == typeof({{type}}))
        {
            int charsWritten;
            while (!(Unsafe.As<T, {{type}}>(ref value)).TryFormat(_chars.Slice(_pos), out charsWritten, default, _provider))
            {
                Grow();
            }

            _pos += charsWritten;
            return;
        }
""";
            sb.AppendLine(code);
        }

        Console.WriteLine(sb.ToString());
    }

    public void Sequence()
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
// This file is generated from FileGen.Command.cs
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

// support for all platforms
// byte/sbyte/ushort/char/short/uint/int/ulong/long (+SIMD optimize)
// float/double/decimal/DateTime/DateTimeOffset

""");

        foreach (var type in PrimitivesForSeqeunceOptimizable.Concat(PrimitivesForSeqeunceOthers))
        {
            var nanValidation = "";
            if (type is "Single" or "Double")
            {
                nanValidation = $$"""
            if ({{type}}.IsNaN(start)) Throws.ArgumentOutOfRange(nameof(start));
            if ({{type}}.IsNaN(endInclusive)) Throws.ArgumentOutOfRange(nameof(endInclusive));
            if ({{type}}.IsNaN(step)) Throws.ArgumentOutOfRange(nameof(step));
""";
            }


            var code = $$"""
namespace ZLinq
{
    public static partial class ValueEnumerable
    {
        public static ValueEnumerable<From{{type}}Sequence, {{type}}> Sequence({{type}} start, {{type}} endInclusive, {{type}} step)
        {
{{nanValidation}}

            if (step == 0) // (T.IsZero(step))
            {
                if (start != endInclusive)
                {
                    Throws.ArgumentOutOfRange(nameof(step));
                }

                // repeat one
                return new(new(start, endInclusive, step, isIncrement: true));
            }
            else if (step >= 0) // (T.IsPositive(step))
            {
                if (endInclusive < start)
                {
                    Throws.ArgumentOutOfRange(nameof(endInclusive));
                }

                // increment pattern
                return new(new(start, endInclusive, step, isIncrement: true));
            }
            else
            {
                if (endInclusive > start)
                {
                    Throws.ArgumentOutOfRange(nameof(endInclusive));
                }

                // decrement pattern
                return new(new(start, endInclusive, step, isIncrement: false));
            }
        }
    }
}

namespace ZLinq.Linq
{
    [StructLayout(LayoutKind.Auto)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct From{{type}}Sequence({{type}} currentValue, {{type}} endInclusive, {{type}} step, bool isIncrement) : IValueEnumerator<{{type}}>
    {
        bool calledGetNext;

        public bool TryGetNext(out {{type}} current)
        {
            if (!calledGetNext)
            {
                calledGetNext = true;
                current = currentValue;
                return true;
            }

            if (isIncrement)
            {
                var next = unchecked(({{type}})(currentValue + step));

                if (next >= endInclusive || next <= currentValue)
                {
                    if (next == endInclusive && currentValue != next)
                    {
                        current = currentValue = next;
                        return true;
                    }

                    current = default!;
                    return false;
                }

                current = currentValue = next;
                return true;
            }
            else
            {
                var next = unchecked(({{type}})(currentValue + step));

                if (next <= endInclusive || next >= currentValue)
                {
                    if (next == endInclusive && currentValue != next)
                    {
                        current = currentValue = next;
                        return true;
                    }

                    current = default!;
                    return false;
                }

                current = currentValue = next;
                return true;
            }
        }

        public void Dispose()
        {
        }
""";

            var optimizedCode = $$"""
        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (CanOptimize())
            {
                count = (int)(endInclusive - currentValue + 1); // currentValue is start if not strated yet.
                return true;
            }

            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<{{type}}> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<{{type}}> destination, Index offset)
        {
            if (TryGetNonEnumeratedCount(out var count))
            {
                if (EnumeratorHelper.TryGetSliceRange(count, offset, destination.Length, out var fillStart, out var fillCount))
                {
                    FillIncremental(destination.Slice(0, fillCount), ({{type}})(currentValue + ({{type}})fillStart));
                    return true;
                }
            }

            return false;
        }

        bool CanOptimize()
        {
            if (step == 1 && endInclusive - currentValue + 1 <= {{type}}.MaxValue)
            {
                return true;
            }
            return false;
        }

        static void FillIncremental(Span<{{type}}> span, {{type}} start)
        {
            ref var current = ref MemoryMarshal.GetReference(span);
            ref var end = ref Unsafe.Add(ref current, span.Length);

#if NET8_0_OR_GREATER
        if (Vector.IsHardwareAccelerated
            && Vector<{{type}}>.IsSupported
#if NET8_0
            && Vector<{{type}}>.Count <= 16
#endif
            && span.Length >= Vector<{{type}}>.Count)
        {
#if NET9_0_OR_GREATER
            var indices = Vector<{{type}}>.Indices;                   // <0, 1, 2, 3, 4, 5, 6, 7>...
#else
            var indices = new Vector<{{type}}>((ReadOnlySpan<{{type}}>)new {{type}}[] { ({{type}})0, ({{type}})1, ({{type}})2, ({{type}})3, ({{type}})4, ({{type}})5, ({{type}})6, ({{type}})7, ({{type}})8, ({{type}})9, ({{type}})10, ({{type}})11, ({{type}})12, ({{type}})13, ({{type}})14, ({{type}})15 });
#endif
            // for example, start = 5, Vector<{{type}}>.Count = 8
            var data = indices + new Vector<{{type}}>(start);         // <5, 6, 7, 8, 9, 10, 11, 12>...
            var increment = new Vector<{{type}}>(({{type}})Vector<{{type}}>.Count);  // <8, 8, 8, 8, 8, 8, 8, 8>...

            ref var to = ref Unsafe.Subtract(ref end, Vector<{{type}}>.Count);
            do
            {
                data.StoreUnsafe(ref current);                              // copy vectorized data to Span pointer
                data += increment;                                          // <13, 14, 15, 16, 17, 18, 19, 20>...
                current = ref Unsafe.Add(ref current, Vector<{{type}}>.Count);   // move pointer++

                // available space for vectorized copy
                // (current <= to) -> !(current > to)
            } while (!Unsafe.IsAddressGreaterThan(ref current, ref to));

            start = data[0]; // next value for fill
        }
#endif

            // fill rest
            while (Unsafe.IsAddressLessThan(ref current, ref end))
            {
                current = start++; // reuse local variable
                current = ref Unsafe.Add(ref current, 1);
            }
        }
""";
            var nonOptimizedCode = $$"""
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<{{type}}> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<{{type}}> destination, Index offset)
        {
            return false;
        }
""";

            var end = $$"""
    }
}
""";

            sb.AppendLine(code);
            if (PrimitivesForSeqeunceOptimizable.Contains(type))
            {
                sb.AppendLine(optimizedCode);
            }
            else
            {
                sb.AppendLine(nonOptimizedCode);
            }
            sb.AppendLine(end);
            sb.AppendLine();
        }

        Console.WriteLine(sb.ToString());
    }



    public void InfiniteSequence()
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
// This file is generated from FileGen.Command.cs
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

// support for all platforms
// byte/sbyte/ushort/char/short/uint/int/ulong/long
// float/double/decimal/DateTime/DateTimeOffset

""");

        foreach (var type in PrimitivesForSeqeunceOptimizable.Concat(PrimitivesForSeqeunceOthers))
        {
            var code = $$"""
namespace ZLinq
{
    public static partial class ValueEnumerable
    {
        public static ValueEnumerable<From{{type}}InfiniteSequence, {{type}}> InfiniteSequence({{type}} start, {{type}} step)
        {
            return new(new(start, step));
        }
    }
}

namespace ZLinq.Linq
{
    [StructLayout(LayoutKind.Auto)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct From{{type}}InfiniteSequence({{type}} start, {{type}} step) : IValueEnumerator<{{type}}>
    {
        bool calledGetNext;

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<{{type}}> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<{{type}}> destination, Index offset)
        {
            return false;
        }

        public bool TryGetNext(out {{type}} current)
        {
            if (!calledGetNext)
            {
                calledGetNext = true;
                current = start;
                return true;
            }

            current = start += step;
            return true;
        }

        public void Dispose()
        {
        }
    }
}
""";

            sb.AppendLine(code);
            sb.AppendLine();
        }

        Console.WriteLine(sb.ToString());
    }
}
