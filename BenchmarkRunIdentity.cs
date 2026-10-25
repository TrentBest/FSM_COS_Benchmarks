using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace FSM_COS_Benchmarks;

/// <summary>A unique invocation label, separate from permanent benchmark-definition IDs.</summary>
internal sealed record BenchmarkRunIdentity(
    string RunId,
    string StartedUtc,
    string CommitSha,
    string WorkflowRunId,
    string WorkflowAttempt)
{
    public static BenchmarkRunIdentity Create()
    {
        var workflowRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "local";
        var workflowAttempt = Environment.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT") ?? "local";
        var commitSha = Environment.GetEnvironmentVariable("GITHUB_SHA")
            ?? Environment.GetEnvironmentVariable("COS_BENCHMARK_COMMIT")
            ?? "unknown";
        var startedUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        var runId = workflowRunId == "local"
            ? $"COSRUN-{DateTimeOffset.UtcNow:yyyyMMddTHHmmssfffZ}-{Guid.NewGuid():N}"[..43]
            : $"COSRUN-{workflowRunId}-{workflowAttempt}-{commitSha[..Math.Min(commitSha.Length, 12)]}";

        return new BenchmarkRunIdentity(runId, startedUtc, commitSha, workflowRunId, workflowAttempt);
    }

    public string ToReport()
    {
        var text = new StringBuilder();
        text.AppendLine($"RunId: {RunId}");
        text.AppendLine($"StartedUtc: {StartedUtc}");
        text.AppendLine($"CommitSha: {CommitSha}");
        text.AppendLine($"WorkflowRunId: {WorkflowRunId}");
        text.AppendLine($"WorkflowAttempt: {WorkflowAttempt}");
        text.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
        text.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        text.AppendLine($"Architecture: {RuntimeInformation.ProcessArchitecture}");
        text.AppendLine($"ProcessorCount: {Environment.ProcessorCount}");
        return text.ToString();
    }
}
