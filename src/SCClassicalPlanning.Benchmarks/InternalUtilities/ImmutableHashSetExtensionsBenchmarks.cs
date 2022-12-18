using BenchmarkDotNet.Attributes;
using SCClassicalPlanning.InternalUtilities;
using System.Collections.Immutable;

namespace SCClassicalPlanning.Benchmarks.InternalUtilities
{
    [MemoryDiagnoser]
    [InProcess]
    public class ImmmutableHashSetExtensionsBenchmarks
    {
        public ImmutableHashSet<int> Set1 { get; } = ImmutableHashSet.Create(Enumerable.Range(0, 100).ToArray());

        public ImmutableHashSet<int> Set2 { get; } = ImmutableHashSet.Create(Enumerable.Range(0, 100).Reverse().ToArray());

        [Benchmark(Baseline = true)]
        public bool SetEqualsDefault() => Set1.SetEquals(Set2);

        [Benchmark]
        public bool SetEqualsWithShortcut() => Set1.SetEquals<int>(Set2);
    }
}
