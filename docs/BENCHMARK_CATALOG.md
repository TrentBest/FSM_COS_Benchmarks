![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# FSM_COS Benchmark Catalog

**Status: initial executable catalog. No performance conclusions have been established.**

This catalog names stable benchmark definitions and defines the identity/evidence contract for each execution. It is separate from package runtime code and unit-test correctness assertions.

## Identity rules

- Definition ID answers what is measured and is permanent: COS-<FAMILY>-<NNN>.
- Run ID answers which invocation produced evidence and must be unique. The harness writes BenchmarkDotNet.Artifacts/BenchmarkRunIdentity.txt on every invocation.
- CI records the harness commit and the FSM_COS source commit separately. A run of one harness revision against another source revision is therefore distinguishable.
- A new semantic operation or materially changed workload receives a new definition ID. Do not reuse or renumber retired IDs; keep retired entries with their retirement reason.
- Preserve the run ID with commit SHAs, runtime, OS, architecture, processor count, benchmark tool version, parameters, build configuration, and raw BenchmarkDotNet artifacts.
- Definition IDs appear in method names and BenchmarkDotNet descriptions so results remain searchable.

## Implemented benchmark definitions

| Stable ID | Benchmark method | Measurement boundary | Workload parameters |
|---|---|---|---|
| COS-MAN-001 | ManifestBenchmarks.COS_MAN_001_BuildRootsAndConstructRuntimeManifest | Allocate root-entry array and construct RuntimeManifest inside the timed method | 1, 10, 100, 1,000 roots |
| COS-MAN-002 | ManifestBenchmarks.COS_MAN_002_ConstructRuntimeManifestFromPreparedRoots | Construct RuntimeManifest from root entries prepared in global setup | 1, 10, 100, 1,000 roots |
| COS-MAN-003 | StagedManifestValidationBenchmarks.COS_MAN_003_ValidateStagedRuntimeManifestLoadPlan | Validate a staged load plan against requested root IDs and versions; all manifest inputs prepared in global setup | 1, 10, 100, 1,000 roots |
| COS-CMP-001 | EmptyManifestBenchmarks.COS_CMP_001_ExecuteEmptyManifest | Execute an empty manifest against an empty in-memory catalog | Empty |
| COS-CMP-002 | CompositionBenchmarks.COS_CMP_002_ComposeIndependentRoots | Execute independent root bundles; catalog, bundles, and manifest prepared in global setup | 1, 10, 100, 1,000 roots |
| COS-CMP-003 | DependencyChainBenchmarks.COS_CMP_003_ComposeDependencyChain | Resolve and compose a linear dependency chain from one requested root; setup excluded | 1, 10, 100 bundles |
| COS-CMP-004 | SharedDependencyBenchmarks.COS_CMP_004_ComposeSharedDependency | Compose multiple requested roots that all depend on one shared bundle | 1, 10, 100, 1,000 roots plus one shared dependency |
| COS-DEP-001 | WideDependencyFanOutBenchmarks.COS_DEP_001_ComposeWideDependencyFanOut | Compose one requested root whose dependency list fans out to independent leaf bundles; graph setup excluded | 1, 10, 100, 1,000 dependencies |
| COS-ERR-001 | FailureBenchmarks.COS_ERR_001_TryExecuteUnresolvedRoot | Run non-throwing composition API for a missing requested root | One missing root |

Timed methods return results to BenchmarkDotNet. MemoryDiagnoser records managed allocation and GC data. ShortRunJob is an initial CI-friendly sampling profile, not a final publication-quality protocol. Re-run apparent regressions with a longer, controlled job before making a performance claim.

## Planned coverage queue

Add each item as a separately named definition only after its workload contract and equivalence checks are explicit.

| Family | Planned coverage |
|---|---|
| COS-MAN | Empty-manifest fast path; staged-plan validation is now covered at 1, 10, 100, and 1,000 roots; immutable snapshot and validation overhead beyond this path |
| COS-CAT | Catalog lookup success/miss, version match/mismatch, dictionary scale and lookup counts |
| COS-DEP | Multiple roots, deep chains, shared dependency closure, duplicate roots, duplicate/conflicting versions |
| COS-LOAD | Load callback cost, configuration retrieval/copy, configuration size, load-order bookkeeping |
| COS-ARB | Stable first-round arbitration, multi-round convergence, near-limit convergence and non-convergence/failure reporting |
| COS-ERR | Request validation, missing dependency, invalid graph/cycle, load failure and rollback/compensation paths |
| COS-RES | Resource bounds, configuration byte bounds, large manifest/dependency inputs |
| COS-E2E | Direct workload versus equivalent MicroBundle composition; startup/composition separated from steady-state execution and teardown |
| COS-CON | Concurrency only where a real concurrent host workload exists; FSM_COS composition is not assumed thread-safe |
| COS-MEM | Allocation and GC pressure across repeated independent composition runs |

## Evidence and comparison rules

1. Do not compare unlike workloads or call composition cost per-tick overhead. FSM_COS composes a runtime; the host owns the steady-state update loop.
2. Keep setup, catalog creation, bundle creation, and manifest creation outside composition measurements unless a benchmark explicitly includes them.
3. Preserve raw CSV/Markdown/HTML and environment evidence; never overwrite previous evidence as if it were the same execution.
4. Record whether the benchmark uses current FSM_COS source or a released package, including both commit/package identity and dependency lane.
5. Initially collect evidence without failing builds on noisy timing thresholds. Add CiAi regression policy only after stable baselines, variance rules, and runner consistency are established.
6. No speedup, complexity claim, or baseline is asserted until the suite has actually run and raw artifacts have been inspected.

## Run locally

~~~powershell
dotnet restore .\FSM_COS_Benchmarks.slnx
dotnet run --project .\FSM_COS_Benchmarks.csproj --configuration Release -- --filter "*COS_*"
~~~

The workflow retains BenchmarkDotNet artifacts, the run identity file, and environment report in a uniquely named artifact. No benchmark project is packed or published.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
