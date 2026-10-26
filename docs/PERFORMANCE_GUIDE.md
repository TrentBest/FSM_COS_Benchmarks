![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Understanding FSM_COS Performance

This guide explains how to interpret the benchmark evidence. It deliberately separates measured facts from hypotheses and performance conclusions.

## The short answer

The first measurements look encouraging for small operations and sub-millisecond composition of up to 1,000 simple roots. They do **not** yet prove FSM_COS is fast relative to alternatives. The current suite uses a three-iteration `ShortRunJob`, and the CI runner is a Hyper-V virtual machine. Treat the current numbers as exploration data, not stable baselines.

## Reading the units

| Unit | Equivalent | Practical interpretation |
|---|---:|---|
| 1 ns | 0.000001 ms | One billionth of a second |
| 1 µs | 1,000 ns | One millionth of a second |
| 1 ms | 1,000 µs | One thousandth of a second |
| 1 MB allocated | 1,024 KB | Managed allocation traffic, not necessarily retained memory |

A rough mental model for a 3–5 GHz CPU is that 1 ns spans about 3–5 clock cycles. This is only a scale illustration, not a conversion from measured nanoseconds to guaranteed CPU cycles: clock frequency changes, the virtualized runner, cache misses, branch prediction, JIT compilation, memory traffic, and scheduling all affect observed time.

Examples from the exploratory run:

- **182.6 ns** for empty-manifest execution is around 0.000183 ms. The latency is tiny, but the roughly 5.07 KB allocated deserves attention if this path runs frequently.
- **22.1 ns** for constructing a one-entry manifest is very small. At this scale, measurement uncertainty and runtime effects can be a meaningful fraction of the result.
- **316.8 µs** for composing 1,000 independent roots is about 0.317 ms. That could be suitable for a startup/reconfiguration operation; it would be a very different concern if repeated every frame.
- **474.5 µs** for 1,000 roots sharing a dependency is about 0.474 ms, with roughly 7.8 MB allocated. The allocation pressure could matter more than the half-millisecond latency under frequent repetition.

These values are not universal constants. They are observations from a particular build, workload, runtime, machine class, and benchmark run.

## Are these numbers good or bad?

A time has no useful pass/fail meaning without a workload and a budget. Ask:

1. **What work is included?** Is the measurement constructing a manifest, validating it, composing a graph, loading bundles, or executing steady-state behavior?
2. **How often does it happen?** Startup work can tolerate costs that are unacceptable on every frame or request.
3. **What is the scale?** Compare one root with one root, 1,000 with 1,000, and equivalent dependency shapes.
4. **What does it allocate?** Allocated bytes per operation multiplied by operation frequency estimates allocation traffic. For example, 5 KB per operation at 1,000 operations per second is roughly 5 MB/s of managed allocation before accounting for other application work.
5. **How certain is the result?** Inspect standard deviation and the 99.9% confidence interval. A wide interval means the displayed mean is not a precise estimate.
6. **What does the application need?** Establish latency, throughput, allocation, and memory budgets from a real use case instead of choosing an arbitrary nanosecond target.

## How FSM_COS compares with known .NET performance numbers

Published dependency-injection benchmarks provide useful context for the *scale* of fast .NET operations, but they are not direct FSM_COS competitors. For example, one third-party C# comparison reports approximately 45 ns for resolving a singleton with Microsoft.Extensions.DependencyInjection and about 2.6 µs to construct a provider. Those are specific to that author's code, environment, and measurement boundaries; they do not establish an industry-wide standard.

A singleton lookup is not runtime composition. Comparing a 45 ns service lookup with a 317 µs composition of 1,000 roots would be misleading: the operations do different amounts of work. The right comparison is a matched workload where implementations produce equivalent results and include equivalent setup, validation, dependency resolution, and allocation boundaries.

Useful reference material:

- [BenchmarkDotNet's benchmarking overview and result interpretation](https://github.com/dotnet/performance/blob/main/docs/benchmarkdotnet.md)
- [BenchmarkDotNet: reliability and measurement caveats](https://github.com/dotnet/BenchmarkDotNet)
- [An illustrative third-party .NET dependency-injection comparison](https://github.com/quinnjr/dependency-injector/blob/main/BENCHMARK_COMPARISON.md)

External numbers are context, not FSM_COS baselines. Hardware, runtime versions, JIT settings, workload semantics, and benchmark design must be checked before drawing comparisons.

## What we should compare next

Build an explicit comparison suite with workload equivalence tests. Each lane should preserve the same output contract and separately report setup, operation latency, allocated bytes, and (where relevant) throughput:

- **Manifest operations:** direct array/list construction versus RuntimeManifest construction and staged validation.
- **Catalog resolution:** dictionary lookup and version check versus the actual catalog implementations used in production.
- **Dependency composition:** hand-written baseline for a narrowly defined graph versus FSM_COS composition of the same graph.
- **Repeated composition:** measure allocation and garbage-collection behavior across many runs, not only one operation.
- **End-to-end:** measure startup/reconfiguration separately from steady-state application updates.

A hand-written baseline is a lower bound for a specific task, not necessarily a feature-equivalent replacement. Any comparison must state what features it omits.

## Current evidence quality

The current CI profile is `ShortRunJob(IterationCount=3, LaunchCount=1, WarmupCount=3)`. It is useful for catching broken workloads and spotting broad scaling behavior, but three measured iterations are not enough for a robust performance claim. The current GitHub Actions Windows runner also reports Hyper-V virtualization, which BenchmarkDotNet warns can affect measurements.

Current policy:

- Keep the stable benchmark definition ID, harness commit, FSM_COS source commit, environment, and raw result artifacts together.
- Treat the current values as exploratory.
- Re-run promising improvements and suspected regressions using longer jobs on a controlled, repeatable runner.
- Establish per-workload baselines only after repeated runs characterize normal variance.
- Do not fail CI on timing changes until the baseline and regression policy can distinguish noise from a real regression.
- Keep composition cost separate from per-frame/per-tick runtime cost. FSM_COS composes a runtime; the host owns the steady-state update loop.

## RuntimeAssembly lookup: position and miss costs

The runtime-lookup suite now separates successful lookup position from misses. Run 40408424494 passed the harness's structural validation: 94 expected rows across 12 CSV reports. The artifact is retained for 90 days.

- [CI run and artifact](https://github.com/TrentBest/FSM_COS_Benchmarks/actions/runs/40408424494)
- [Harness commit](https://github.com/TrentBest/FSM_COS_Benchmarks/commit/2e6741438d960b6e12ed07cecf775f953d84794c)
- [FSM_COS indexed-lookup investigation](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/issues/42)

Exploratory means for untyped ID lookup:

| Bundle count | First ID | Middle ID | Missing ID |
|---:|---:|---:|---:|
| 1 | 27.6 ns | 27.4 ns | 28.7 ns |
| 10 | 27.9 ns | 42.2 ns | 60.6 ns |
| 100 | 27.8 ns | 209.2 ns | 395.5 ns |
| 1,000 | 27.9 ns | 1.65 µs | 3.46 µs |

Typed lookup by ID shows a larger scan cost in this fixture:

| Bundle count | Typed first ID | Typed middle ID | Typed missing ID |
|---:|---:|---:|---:|
| 1 | 56.2 ns | 56.1 ns | 62.7 ns |
| 10 | 56.9 ns | 99.4 ns | 165.7 ns |
| 100 | 58.1 ns | 615.9 ns | 1.24 µs |
| 1,000 | 53.3 ns | 5.22 µs | 10.54 µs |

The reported allocation is 120 B/op for untyped ID lookup and 176 B/op for typed ID lookup, independent of target position in this run. These values describe the current implementation and benchmark fixture, not the cost of a future indexed implementation.

**Interpretation:** early hits remain almost flat as assembly size grows, while middle hits and misses rise with bundle count. The typed overload is more expensive because it filters by type (OfType<TBundle>) before matching the ID. At 1,000 bundles, a typed miss is about 10.5 µs versus 3.46 µs for an untyped miss.

**Next decision:** evaluate a private ID index built once per RuntimeAssembly, but only after confirming duplicate-ID semantics, deterministic behavior, immutability, and retained-memory cost. A dictionary lookup in a benchmark is a useful comparison point, not proof that an index is automatically worth its construction and memory cost. The current three-iteration ShortRun and virtualized runner make these results an optimization signal, not a production guarantee.

## The metaDev direction

The long-term objective is for every component to have a verifiable performance profile: stable workload identity, declared environment, latency distribution, allocation and GC costs, scaling curves, and comparison against equivalent alternatives. A future metaDev tool can collect this evidence across projects and versions, flag statistically credible regressions, and show the trade-off between performance and capabilities.

The evidence should answer not merely "is it fast?" but "fast for what, under which conditions, at what cost, and compared to which equivalent implementation?"

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
