using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

namespace FSM_COS_Benchmarks;

/// <summary>
/// Measures steady-state retrieval from an already composed assembly. Composition and the
/// direct dictionary index are prepared outside the timed methods. The dictionary and list
/// cases are host-side reference implementations, not additional FSM_COS APIs.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class RuntimeLookupBenchmarks
{
    private RuntimeAssembly _assembly = null!;
    private Dictionary<ulong, IMicroBundle> _directIndex = null!;
    private ulong _targetId;

    [Params(1, 10, 100, 1000)]
    public int BundleCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = BenchmarkWorkloads.IndependentGraph(BundleCount);
        _assembly = new FsmCos(graph.Catalog).Execute(graph.Manifest);
        _directIndex = _assembly.Bundles.ToDictionary(bundle => bundle.Id);
        // Search for the final ID to expose the scaling of RuntimeAssembly's linear lookup.
        _targetId = (ulong)BundleCount;
    }

    [Benchmark(Description = "COS-RT-001 RuntimeAssembly TryGetBundle")]
    public bool COS_RT_001_RuntimeAssemblyTryGetBundle() =>
        _assembly.TryGetBundle(_targetId, out _);

    [Benchmark(Description = "COS-RT-002 Host dictionary lookup")]
    public bool COS_RT_002_HostDictionaryLookup() =>
        _directIndex.TryGetValue(_targetId, out _);

    [Benchmark(Description = "COS-RT-004 RuntimeAssembly typed lookup by ID")]
    public bool COS_RT_004_RuntimeAssemblyTypedLookupById() =>
        _assembly.TryGetBundle<BenchmarkBundle>(_targetId, out _);

    [Benchmark(Description = "COS-RT-005 RuntimeAssembly typed lookup by type")]
    public bool COS_RT_005_RuntimeAssemblyTypedLookupByType() =>
        _assembly.TryGetBundle<BenchmarkBundle>(out _);

    [Benchmark(Description = "COS-RT-003 Host linear bundle scan")]
    public bool COS_RT_003_HostLinearBundleScan()
    {
        foreach (var bundle in _assembly.Bundles)
        {
            if (bundle.Id == _targetId)
                return true;
        }

        return false;
    }
}
