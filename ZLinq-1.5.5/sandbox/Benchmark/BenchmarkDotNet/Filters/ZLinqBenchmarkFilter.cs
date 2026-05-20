using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using System.Text.RegularExpressions;

namespace Benchmark;

/// <summary>
/// Custom filter that filter benchmarks with `Benchmark.ZLinq.*` namespace and `ZLinqOnly` category.
/// </summary>
public partial class ZLinqBenchmarkFilter : IFilter
{
    public virtual bool Predicate(BenchmarkCase benchmarkCase)
    {
        // Filter by namespace
        var ns = benchmarkCase.Descriptor.Type.Namespace!;
        if (!ns.StartsWith("Benchmark.ZLinq"))
            return false;

        switch (benchmarkCase.Job.Id)
        {
            // Filter by `ZLinqOnly` category for SystemLinq benchmarks.
            case SystemLinqBenchmarkConfig.SystemLinqJobId:
                var categories = benchmarkCase.Descriptor.Categories;
                return !categories.Contains(Categories.Filters.ZLinqOnly);
            default:
                return true;
        }
    }
}
