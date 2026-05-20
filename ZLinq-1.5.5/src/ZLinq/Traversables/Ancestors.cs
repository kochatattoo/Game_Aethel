namespace ZLinq.Traversables;

[StructLayout(LayoutKind.Auto)]
public struct Ancestors<TTraverser, T>(TTraverser traverser, bool withSelf)
    : IValueEnumerator<T>
    where TTraverser : struct, ITraverser<TTraverser, T>
{
    public bool TryGetNonEnumeratedCount(out int count)
    {
        count = 0;
        return false;
    }

    public bool TryGetSpan(out ReadOnlySpan<T> span)
    {
        span = default;
        return false;
    }

    public bool TryCopyTo(Span<T> destination, Index offset) => false;

    public bool TryGetNext(out T current)
    {
        if (withSelf)
        {
            current = traverser.Origin;
            withSelf = false;
            return true;
        }

        if (traverser.TryGetParent(out var parent))
        {
            current = parent;
            var nextTraverser = traverser.ConvertToTraverser(parent);
            traverser.Dispose();
            traverser = nextTraverser;
            return true;
        }

        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        traverser.Dispose();
    }
}
