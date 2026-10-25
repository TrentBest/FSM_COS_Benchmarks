using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class FailureBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _missingRoot = null!;

    [GlobalSetup]
    public void Setup()
    {
        _composer = new FsmCos(new BenchmarkCatalog(Array.Empty<BenchmarkBundle>()));
        _missingRoot = new RuntimeManifest(1, new[] { new MicroBundleManifestEntry(404, "1.0.0-benchmark") });
    }

    [Benchmark(Description = "COS-ERR-001 TryExecute unresolved root")]
    public RuntimeCompositionResult COS_ERR_001_TryExecuteUnresolvedRoot() =>
        _composer.TryExecute(_missingRoot);
}
