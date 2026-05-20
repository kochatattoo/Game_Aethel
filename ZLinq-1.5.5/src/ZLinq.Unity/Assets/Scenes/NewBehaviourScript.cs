using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using ZLinq;
using ZLinq.Linq;

[assembly: ZLinqDropInAttribute("ZLinq", ZLinq.DropInGenerateTypes.Everything)]
[assembly: ZLinqDropInExternalExtension("ZLinq", "System.Collections.Generic.IReadOnlyCollection`1")]
[assembly: ZLinqDropInExternalExtension("ZLinq", "System.Collections.Generic.IReadOnlyList`1")]
[assembly: ZLinqDropInExternalExtension("ZLinq", "Unity.Collections.NativeArray`1", "ZLinq.Linq.FromNativeArray`1")]
[assembly: ZLinqDropInExternalExtension("ZLinq", "Unity.Collections.NativeArray`1+ReadOnly", "ZLinq.Linq.FromNativeArray`1")]
[assembly: ZLinqDropInExternalExtension("ZLinq", "Unity.Collections.NativeSlice`1", "ZLinq.Linq.FromNativeSlice`1")]
[assembly: ZLinqDropInExternalExtension("ZLinq", "Unity.Collections.NativeList`1", "ZLinq.Linq.FromNativeList`1")]






















public class NewBehaviourScript : MonoBehaviour
{
    public GameObject Origin;

    void Start()
    {
        var temparray = new NativeArray<int>(10, Allocator.Temp);
        ValueEnumerable.Range(1, 10).CopyTo(temparray);

        var xs = temparray.Select(x => x).Shuffle();
        using var e = xs.Enumerator;
        while (e.TryGetNext(out var current))
        {
            Debug.Log(current);
        }

        Debug.Log("----");

        var templist = new NativeList<int>(10, Allocator.Temp);
        templist.Resize(10, NativeArrayOptions.ClearMemory);
        ValueEnumerable.Range(1, 10).CopyTo(templist.AsArray());

        var xs2 = templist.Select(x => x).Shuffle();
        using var e2 = xs2.Enumerator;
        while (e2.TryGetNext(out var current))
        {
            Debug.Log(current);
        }


        //for (int i = 0; i < 10000; i++)
        //{
        //    if (i % 100 == 0) Debug.Log(i);
        //    Enumerable.Range(1, i).AsValueEnumerable().ToList();
        //    Enumerable.Range(1, i).ToArray().AsValueEnumerable().ToList();
        //}

        //Test();
        //Test2();
    }

    IEnumerable<int> Iterate()
    {
        yield return 42;
    }

    public static void Test()
    {
        var tako = ValueEnumerable.Range(1, 10).Select(x => x.ToString());
        var str = string.Join(',', tako.AsEnumerable());
        Debug.Log(str);
    }

    public static void Test2()
    {
        var w = ValueEnumerable.Range(1, 10)
            .Where(x => x % 2 == 0)
            .Take(10)
            .Index()
            .Order()
            .Skip(1)
            .Shuffle()
            .Select(x => x.Item)
            .Prepend(9999)
            .Append(10000)
            .Chunk(99)
            .Distinct();

        foreach (var item in w)
        {
            Debug.Log(item);
        }
    }
}

public static class ZLinqExtensions
{
    public static IEnumerable<T> AsEnumerable<TEnumerator, T>(this ValueEnumerable<TEnumerator, T> valueEnumerable)
        where TEnumerator : struct, IValueEnumerator<T>
    {
        using (var e = valueEnumerable.Enumerator)
        {
            while (e.TryGetNext(out var current))
            {
                yield return current;
            }
        }
    }
}
