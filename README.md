![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# FSM_COS Benchmarks

A dedicated, repeatable performance laboratory for **FSM_COS**: manifest construction, runtime composition, dependency graph shapes, failure paths, allocations, and scaling behavior.

**Current status:** the initial suite has completed a real CI run against FSM_COS development source. Its measurements are exploratory, not a performance baseline. The suite is expanding by stable-ID workload families; newly added cases must pass CI and have their artifacts reviewed before being considered validated.

## Design goals

- **Stable benchmark IDs** identify what is measured; **unique run IDs** identify each execution.
- Benchmark current FSM_COS development source in CI, rather than accidentally measuring only the released package.
- Keep setup out of the timed region unless setup is explicitly part of the benchmark contract.
- Capture allocation/GC data and preserve raw BenchmarkDotNet reports with environment metadata.
- Add coverage deliberately across each public subsystem and meaningful complexity boundary.
- Collect evidence before setting regression thresholds. Do not fail builds on noisy timing changes until baselines and variance rules are defensible.
- Never publish packages from this repository.

## Quick start

Requires the .NET 8 SDK.

~~~powershell
dotnet restore .\FSM_COS_Benchmarks.slnx
dotnet run --project .\FSM_COS_Benchmarks.csproj --configuration Release -- --filter "*COS_*"
~~~

By default, local execution uses the released TheSingularityWorkshop.FSM_COS NuGet package. To benchmark a local development checkout, pass the project path explicitly:

~~~powershell
dotnet run --project .\FSM_COS_Benchmarks.csproj --configuration Release -p:FsmCosSourceProject="..\..\TheSingularityWorkshop.FSM_COS\src\FSM_COS\FSM_COS.csproj" -- --filter "*COS_*"
~~~

The source project can still use its declared package dependencies; this overrides only which FSM_COS project is under test.

## Evidence

Every run writes BenchmarkDotNet.Artifacts/BenchmarkRunIdentity.txt. CI also records both the benchmark-harness commit and the FSM_COS source commit, plus .NET, OS, CPU, and runner details. Workflow artifacts are named by run ID, attempt, and harness commit so separate executions are not silently conflated.

See [the benchmark catalog](docs/BENCHMARK_CATALOG.md) for permanent IDs, timed boundaries, parameters, and the planned coverage queue. Read [Understanding FSM_COS Performance](docs/PERFORMANCE_GUIDE.md) for practical interpretation of nanoseconds, allocations, uncertainty, and fair comparisons with other implementations.

## Repository policy

- master represents the last accepted/released state; development is the active work branch.
- No NuGet publishing is performed by this repository.
- A passing benchmark run means the harness completed, not that performance has been proven stable.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
