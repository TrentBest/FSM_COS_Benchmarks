using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class WideDependencyFanOutBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [Params(1, 10, 100, 1000)]
    public int DependencyCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.WideDependencyFanOut(DependencyCount);
        _composer = new FsmCos(graph.Catalog);
        _manifest = graph.Manifest;
    }

    [Benchmark(Description = "COS-DEP-001 Compose one root with wide dependency fan-out")]
    public RuntimeAssembly COS_DEP_001_ComposeWideDependencyFanOut() =>
        _composer.Execute(_manifest);
}
