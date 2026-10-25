using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ManifestBenchmarks
{
    private MicroBundleManifestEntry[] _roots = Array.Empty<MicroBundleManifestEntry>();

    [Params(1, 10, 100, 1000)]
    public int BundleCount { get; set; }

    [GlobalSetup]
    public void Setup() =>
        _roots = Enumerable.Range(1, BundleCount)
            .Select(index => new MicroBundleManifestEntry((ulong)index, "1.0.0-benchmark"))
            .ToArray();

    [Benchmark(Description = "COS-MAN-001 Build roots and construct runtime manifest")]
    public RuntimeManifest COS_MAN_001_BuildRootsAndConstructRuntimeManifest()
    {
        var roots = Enumerable.Range(1, BundleCount)
            .Select(index => new MicroBundleManifestEntry((ulong)index, "1.0.0-benchmark"))
            .ToArray();
        return new RuntimeManifest(1, roots);
    }

    [Benchmark(Description = "COS-MAN-002 Construct runtime manifest from prepared roots")]
    public RuntimeManifest COS_MAN_002_ConstructRuntimeManifestFromPreparedRoots() =>
        new(1, _roots);
}
