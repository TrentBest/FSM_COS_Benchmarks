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
    private ulong _firstTargetId;
    private ulong _middleTargetId;
    private ulong _missingTargetId;

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
        _firstTargetId = 1;
        _middleTargetId = (ulong)((BundleCount + 1) / 2);
        _missingTargetId = (ulong)BundleCount + 1;
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

    [Benchmark(Description = "COS-RT-006 RuntimeAssembly lookup first ID")]
    public bool COS_RT_006_RuntimeAssemblyLookupFirstId() =>
        _assembly.TryGetBundle(_firstTargetId, out _);

    [Benchmark(Description = "COS-RT-007 RuntimeAssembly lookup middle ID")]
    public bool COS_RT_007_RuntimeAssemblyLookupMiddleId() =>
        _assembly.TryGetBundle(_middleTargetId, out _);

    [Benchmark(Description = "COS-RT-008 RuntimeAssembly lookup missing ID")]
    public bool COS_RT_008_RuntimeAssemblyLookupMissingId() =>
        _assembly.TryGetBundle(_missingTargetId, out _);

    [Benchmark(Description = "COS-RT-009 RuntimeAssembly typed lookup first ID")]
    public bool COS_RT_009_RuntimeAssemblyTypedLookupFirstId() =>
        _assembly.TryGetBundle<BenchmarkBundle>(_firstTargetId, out _);

    [Benchmark(Description = "COS-RT-010 RuntimeAssembly typed lookup middle ID")]
    public bool COS_RT_010_RuntimeAssemblyTypedLookupMiddleId() =>
        _assembly.TryGetBundle<BenchmarkBundle>(_middleTargetId, out _);

    [Benchmark(Description = "COS-RT-011 RuntimeAssembly typed lookup missing ID")]
    public bool COS_RT_011_RuntimeAssemblyTypedLookupMissingId() =>
        _assembly.TryGetBundle<BenchmarkBundle>(_missingTargetId, out _);

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
