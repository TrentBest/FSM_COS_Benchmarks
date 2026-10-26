using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;

namespace FSM_COS_Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class StagedManifestValidationBenchmarks
{
    private RuntimeManifest _manifest = null!;

    [Params(1, 10, 100, 1000)]
    public int BundleCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var roots = Enumerable.Range(1, BundleCount)
            .Select(index => new MicroBundleManifestEntry((ulong)index, "1.0.0-benchmark"))
            .ToArray();

        var loadPlan = roots
            .Select(root => new RuntimeManifestEntry(
                new MicroBundleReference(root.BundleId, root.Version, "benchmark-content-hash"),
                ManifestLoadStage.Resident))
            .ToArray();

        _manifest = new RuntimeManifest(1, roots, LoadPlan: loadPlan);
    }

    [Benchmark(Description = "COS-MAN-003 Validate staged runtime manifest load plan")]
    public void COS_MAN_003_ValidateStagedRuntimeManifestLoadPlan() =>
        _manifest.ValidateStagedPlan();
}
