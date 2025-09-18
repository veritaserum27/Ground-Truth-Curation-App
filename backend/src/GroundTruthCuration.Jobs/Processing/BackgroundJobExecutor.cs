using GroundTruthCuration.Jobs.Entities;
using GroundTruthCuration.Jobs.Services;

namespace GroundTruthCuration.Jobs.Processing;

/// <summary>
/// Simple executor that simulates work for each job type.
/// </summary>
public class BackgroundJobExecutor : IBackgroundJobExecutor
{
    /// <inheritdoc />
    public async Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken)
    {
        // Simulate variable work; in real implementation delegate to appropriate domain service.
        var totalSteps = 5;
        for (int i = 1; i <= totalSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(300, cancellationToken).ConfigureAwait(false);
            var pct = (int)Math.Round(i * 100.0 / totalSteps);
            progressCallback?.Invoke(pct, $"Step {i} of {totalSteps}...");
        }
        // Produce a trivial JSON result dependent on job type.
        string result = job.Type switch
        {
            BackgroundJobType.Export => $"{{\"message\":\"Export completed\",\"jobId\":\"{job.Id}\"}}",
            BackgroundJobType.DataQueryExecution => $"{{\"message\":\"Query executed\",\"rows\":42,\"jobId\":\"{job.Id}\"}}",
            BackgroundJobType.ResponseGeneration => $"{{\"message\":\"Response generated\",\"jobId\":\"{job.Id}\"}}",
            _ => $"{{\"message\":\"Job completed\",\"jobId\":\"{job.Id}\"}}"
        };
        return result;
    }
}
