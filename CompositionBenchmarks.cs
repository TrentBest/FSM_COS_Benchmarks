using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class CompositionBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [Params(1, 10, 100, 1000)]
    public int BundleCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.IndependentGraph(BundleCount);
        _composer = new FsmCos(graph.Catalog);
        _manifest = graph.Manifest;
    }

    [Benchmark(Description = "COS-CMP-002 Compose independent roots")]
    public RuntimeAssembly COS_CMP_002_ComposeIndependentRoots() =>
        _composer.Execute(_manifest);
}

[MemoryDiagnoser]
[ShortRunJob]
public class EmptyManifestBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _composer = new FsmCos(new BenchmarkCatalog(Array.Empty<BenchmarkBundle>()));
        _manifest = RuntimeManifest.Empty(1);
    }

    [Benchmark(Description = "COS-CMP-001 Execute empty manifest")]
    public RuntimeAssembly COS_CMP_001_ExecuteEmptyManifest() =>
        _composer.Execute(_manifest);
}

[MemoryDiagnoser]
[ShortRunJob]
public class DependencyChainBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [Params(1, 10, 100)]
    public int BundleCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.DependencyChain(BundleCount);
        _composer = new FsmCos(graph.Catalog);
        _manifest = graph.Manifest;
    }

    [Benchmark(Description = "COS-CMP-003 Compose dependency chain")]
    public RuntimeAssembly COS_CMP_003_ComposeDependencyChain() =>
        _composer.Execute(_manifest);
}

[MemoryDiagnoser]
[ShortRunJob]
public class SharedDependencyBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [Params(1, 10, 100, 1000)]
    public int RootCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.SharedDependencyGraph(RootCount);
        _composer = new FsmCos(graph.Catalog);
        _manifest = graph.Manifest;
    }

    [Benchmark(Description = "COS-CMP-004 Compose roots with shared dependency")]
    public RuntimeAssembly COS_CMP_004_ComposeSharedDependency() =>
        _composer.Execute(_manifest);
}
