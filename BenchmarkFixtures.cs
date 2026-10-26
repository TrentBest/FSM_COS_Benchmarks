using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

namespace FSM_COS_Benchmarks;

internal sealed class BenchmarkCatalog : IMicroBundleCatalog
{
    private readonly Dictionary<ulong, IMicroBundle> _bundles;

    public BenchmarkCatalog(IEnumerable<IMicroBundle> bundles) =>
        _bundles = bundles.ToDictionary(bundle => bundle.Id);

    public bool TryResolve(ulong bundleId, string version, out IMicroBundle? bundle) =>
        _bundles.TryGetValue(bundleId, out bundle) &&
        string.Equals(bundle.Descriptor.Version, version, StringComparison.Ordinal);

    public bool TryResolve(ulong bundleId, out IMicroBundle? bundle) =>
        _bundles.TryGetValue(bundleId, out bundle);
}

internal sealed class BenchmarkBundle : IMicroBundle
{
    private readonly IReadOnlyList<MicroBundleDependencyRequest> _dependencies;

    public BenchmarkBundle(ulong id, IEnumerable<ulong>? dependencyIds = null)
    {
        Id = id;
        var ids = (dependencyIds ?? Array.Empty<ulong>()).ToArray();
        _dependencies = ids.Select(MicroBundleDependencyRequest.Unconfigured).ToArray();
        Descriptor = new MicroBundleDescriptor(
            id,
            "1.0.0-benchmark",
            ids.Select(dependencyId => new MicroBundleDependency(dependencyId)));
    }

    public ulong Id { get; }
    public MicroBundleDescriptor Descriptor { get; }
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => _dependencies;

    public void Load(IMicroBundleLoadContext context) { }

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex) => false;
}

internal static class BenchmarkWorkloads
{
    public static RuntimeManifest IndependentRoots(int bundleCount)
    {
        var roots = Enumerable.Range(1, bundleCount)
            .Select(index => new MicroBundleManifestEntry((ulong)index, "1.0.0-benchmark"))
            .ToArray();
        return new RuntimeManifest(1, roots);
    }

    public static (RuntimeManifest Manifest, BenchmarkCatalog Catalog) IndependentGraph(int bundleCount)
    {
        var bundles = Enumerable.Range(1, bundleCount)
            .Select(index => new BenchmarkBundle((ulong)index))
            .ToArray();
        return (IndependentRoots(bundleCount), new BenchmarkCatalog(bundles));
    }

    public static (RuntimeManifest Manifest, BenchmarkCatalog Catalog) DependencyChain(int bundleCount)
    {
        var bundles = new List<BenchmarkBundle>(bundleCount);
        for (var index = 1; index <= bundleCount; index++)
        {
            var dependencies = index == bundleCount
                ? Array.Empty<ulong>()
                : new[] { (ulong)(index + 1) };
            bundles.Add(new BenchmarkBundle((ulong)index, dependencies));
        }

        var manifest = new RuntimeManifest(
            1,
            new[] { new MicroBundleManifestEntry(1, "1.0.0-benchmark") });
        return (manifest, new BenchmarkCatalog(bundles));
    }

    public static (RuntimeManifest Manifest, BenchmarkCatalog Catalog) SharedDependencyGraph(int rootCount)
    {
        var sharedId = (ulong)rootCount + 1;
        var bundles = Enumerable.Range(1, rootCount)
            .Select(index => new BenchmarkBundle((ulong)index, new[] { sharedId }))
            .Append(new BenchmarkBundle(sharedId))
            .ToArray();
        return (IndependentRoots(rootCount), new BenchmarkCatalog(bundles));
    }

    public static (RuntimeManifest Manifest, BenchmarkCatalog Catalog) WideDependencyFanOut(int dependencyCount)
    {
        var dependencyIds = Enumerable.Range(2, dependencyCount)
            .Select(index => (ulong)index)
            .ToArray();
        var bundles = new List<BenchmarkBundle>(dependencyCount + 1)
        {
            new(1, dependencyIds)
        };
        bundles.AddRange(dependencyIds.Select(id => new BenchmarkBundle(id)));

        var manifest = new RuntimeManifest(
            1,
            new[] { new MicroBundleManifestEntry(1, "1.0.0-benchmark") });
        return (manifest, new BenchmarkCatalog(bundles));
    }
}
