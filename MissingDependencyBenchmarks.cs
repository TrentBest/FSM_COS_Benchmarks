using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class MissingDependencyBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _missingDependency = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Root 1 exists, but its dependency 2 is intentionally absent from the catalog.
        var root = new BenchmarkBundle(1, new[] { 2UL });
        _composer = new FsmCos(new BenchmarkCatalog(new[] { root }));
        _missingDependency = new RuntimeManifest(
            1,
            new[] { new MicroBundleManifestEntry(1, "1.0.0-benchmark") });
    }

    [Benchmark(Description = "COS-ERR-002 TryExecute missing dependency")]
    public RuntimeCompositionResult COS_ERR_002_TryExecuteMissingDependency() =>
        _composer.TryExecute(_missingDependency);
}
