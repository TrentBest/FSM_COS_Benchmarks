using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

namespace FSM_COS_Benchmarks;

/// <summary>
/// Measures composition with real bundle Load callbacks that mutate bundle-local state.
/// The callback is intentionally small; this is a lower-bound callback workload, not an
/// application-specific loading benchmark.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class LoadCallbackBenchmarks
{
    private FsmCos _composer = null!;
    private RuntimeManifest _manifest = null!;
    private LoadProbeBundle[] _bundles = Array.Empty<LoadProbeBundle>();

    [Params(1, 10, 100, 1000)]
    public int RootCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _bundles = Enumerable.Range(1, RootCount)
            .Select(index => new LoadProbeBundle((ulong)index))
            .ToArray();

        _composer = new FsmCos(new BenchmarkCatalog(_bundles));
        _manifest = BenchmarkWorkloads.IndependentRoots(RootCount);
    }

    [Benchmark(Description = "COS-LOAD-001 Compose roots with stateful Load callbacks")]
    public RuntimeCompositionResult COS_LOAD_001_ComposeRootsWithStatefulLoadCallbacks() =>
        _composer.TryExecute(_manifest);
}

internal sealed class LoadProbeBundle : IMicroBundle
{
    private readonly IReadOnlyList<MicroBundleDependencyRequest> _dependencies =
        Array.Empty<MicroBundleDependencyRequest>();

    public LoadProbeBundle(ulong id)
    {
        Id = id;
        Descriptor = new MicroBundleDescriptor(id, "1.0.0-benchmark");
    }

    public ulong Id { get; }
    public MicroBundleDescriptor Descriptor { get; }
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => _dependencies;

    // State mutation makes the callback a real, observable workload rather than an empty method.
    public long LoadCount { get; private set; }

    public void Load(IMicroBundleLoadContext context) => LoadCount++;

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex) => false;
}
