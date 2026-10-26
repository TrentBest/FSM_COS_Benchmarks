using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace FSM_COS_Benchmarks;

/// <summary>
/// Isolates the in-memory benchmark catalog lookup contract from composition overhead.
/// This dictionary-backed catalog is a lower-bound fixture, not a claim about every host catalog.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class CatalogLookupBenchmarks
{
    private BenchmarkCatalog _catalog = null!;
    private ulong _hitId;
    private ulong _missId;

    [Params(1, 10, 100, 1000)]
    public int CatalogSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.IndependentGraph(CatalogSize);
        _catalog = graph.Catalog;
        _hitId = (ulong)CatalogSize;
        _missId = (ulong)CatalogSize + 1;
    }

    [Benchmark(Description = "COS-CAT-001 Resolve existing bundle by ID")]
    public bool COS_CAT_001_ResolveExistingBundleById() =>
        _catalog.TryResolve(_hitId, out _);

    [Benchmark(Description = "COS-CAT-002 Resolve existing bundle by matching version")]
    public bool COS_CAT_002_ResolveExistingBundleByMatchingVersion() =>
        _catalog.TryResolve(_hitId, "1.0.0-benchmark", out _);

    [Benchmark(Description = "COS-CAT-003 Miss absent bundle ID")]
    public bool COS_CAT_003_MissAbsentBundleId() =>
        _catalog.TryResolve(_missId, out _);

    [Benchmark(Description = "COS-CAT-004 Reject version mismatch")]
    public bool COS_CAT_004_RejectVersionMismatch() =>
        _catalog.TryResolve(_hitId, "9.9.9-missing", out _);
}
