using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLinq.Tests.Linq;

public class ListSelectTest
{
    [Fact]
    public void Select()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Select(x => x * 10).ToArray();

        var list2 = new List<int>();
        foreach (var item in list.AsValueEnumerable().Select(x => x * 10))
        {
            list2.Add(item);
        }

        list2.ToArray().ShouldBe(expected);

        var dest = new int[10];
        list.AsValueEnumerable().Select(x => x * 10).Enumerator.TryCopyTo(dest, 0);
        dest.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SelectWhere()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Select(x => x * 10).Where(x => x <= 50).ToArray();

        var list2 = new List<int>();
        foreach (var item in list.AsValueEnumerable().Select(x => x * 10).Where(x => x <= 50))
        {
            list2.Add(item);
        }

        list2.ToArray().ShouldBe(expected);
    }


    [Fact]
    public void Where()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Where(x => x % 2 == 0).ToArray();

        var list2 = new List<int>();
        foreach (var item in list.AsValueEnumerable().Where(x => x % 2 == 0))
        {
            list2.Add(item);
        }

        list2.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void WhereSelect()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Where(x => x % 2 == 0).Select(x => x * 10).ToArray();

        var list2 = new List<int>();
        foreach (var item in list.AsValueEnumerable().Where(x => x % 2 == 0).Select(x => x * 10))
        {
            list2.Add(item);
        }

        list2.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SelectToArray()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Select(x => x * 10).ToArray();

        list.AsValueEnumerable().Select(x => x * 10).ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SelectToList()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Select(x => x * 10).ToList();

        list.AsValueEnumerable().Select(x => x * 10).ToList().ShouldBe(expected);
    }

    [Fact]
    public void WhereToArray()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = list.Where(x => x % 2 == 0).ToArray();

        list.AsValueEnumerable().Where(x => x % 2 == 0).ToArray().ShouldBe(expected);
    }
}
