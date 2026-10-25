using BenchmarkDotNet.Running;

namespace FSM_COS_Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        var run = BenchmarkRunIdentity.Create();
        Console.WriteLine($"FSM_COS benchmark run: {run.RunId}");
        Console.WriteLine($"Benchmark repository commit: {run.CommitSha}");
        Console.WriteLine($"Runtime: {Environment.Version}; OS: {Environment.OSVersion}; Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

        Directory.CreateDirectory("BenchmarkDotNet.Artifacts");
        File.WriteAllText(
            Path.Combine("BenchmarkDotNet.Artifacts", "BenchmarkRunIdentity.txt"),
            run.ToReport());

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
