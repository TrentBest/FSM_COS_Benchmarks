using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class DeepDependencyChainBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [Params(100, 1000, 10000)]
    public int BundleCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.DependencyChain(BundleCount);
        _composer = new FsmCos(graph.Catalog);
        _manifest = graph.Manifest;

        var result = _composer.TryExecute(_manifest);
        if (!result.Succeeded || result.Assembly?.Bundles.Count != BundleCount)
        {
            throw new InvalidOperationException(
                $"Deep-chain fixture expected {BundleCount} composed bundles; " +
                $"succeeded={result.Succeeded}, actual={result.Assembly?.Bundles.Count ?? 0}, " +
                $"phase={result.FailurePhase}.");
        }
    }

    [Benchmark(Description = "COS-DEP-002 Compose deep dependency chain")]
    public RuntimeCompositionResult COS_DEP_002_ComposeDeepDependencyChain() =>
        _composer.TryExecute(_manifest);
}

[MemoryDiagnoser]
[ShortRunJob]
public class DuplicateRootRequestBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [Params(10, 100, 1000)]
    public int RequestCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var bundle = new BenchmarkBundle(1);
        _composer = new FsmCos(new BenchmarkCatalog(new[] { bundle }));
        var roots = Enumerable.Repeat(
                new MicroBundleManifestEntry(1, "1.0.0-benchmark"),
                RequestCount)
            .ToArray();
        _manifest = new RuntimeManifest(1, roots);

        var result = _composer.TryExecute(_manifest);
        if (!result.Succeeded || result.Assembly?.Bundles.Count != 1)
        {
            throw new InvalidOperationException(
                $"Duplicate-root fixture expected one composed bundle; " +
                $"succeeded={result.Succeeded}, actual={result.Assembly?.Bundles.Count ?? 0}, " +
                $"phase={result.FailurePhase}.");
        }
    }

    [Benchmark(Description = "COS-CMP-005 Deduplicate repeated identical root requests")]
    public RuntimeCompositionResult COS_CMP_005_DeduplicateRepeatedIdenticalRootRequests() =>
        _composer.TryExecute(_manifest);
}

[MemoryDiagnoser]
[ShortRunJob]
public class ConflictingRootVersionBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _composer = new FsmCos(new BenchmarkCatalog(new[] { new BenchmarkBundle(1) }));
        _manifest = new RuntimeManifest(
            1,
            new[]
            {
                new MicroBundleManifestEntry(1, "1.0.0-benchmark"),
                new MicroBundleManifestEntry(1, "2.0.0-benchmark")
            });

        var result = _composer.TryExecute(_manifest);
        if (result.Succeeded || result.FailurePhase != RuntimeCompositionPhase.RequestValidation)
        {
            throw new InvalidOperationException(
                $"Conflicting-root fixture expected RequestValidation failure; " +
                $"succeeded={result.Succeeded}, phase={result.FailurePhase}.");
        }
    }

    [Benchmark(Description = "COS-ERR-003 Reject conflicting versions for one root ID")]
    public RuntimeCompositionResult COS_ERR_003_RejectConflictingRootVersions() =>
        _composer.TryExecute(_manifest);
}
