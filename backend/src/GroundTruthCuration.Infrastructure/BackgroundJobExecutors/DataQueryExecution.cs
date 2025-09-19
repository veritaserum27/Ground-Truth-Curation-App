using System.Text.Json;
using GroundTruthCuration.Jobs.Entities;
using GroundTruthCuration.Jobs.Processing;

namespace GroundTruthCuration.Infrastructure.BackgroundJobExecutors;

public class DataQueryExecution : IBackgroundJobExecutor
{
    public async Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken)
    {
        var totalSteps = 25;
        for (int i = 1; i <= totalSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(800, cancellationToken).ConfigureAwait(false);
            var pct = (int)Math.Round(i * 100.0 / totalSteps);
            progressCallback?.Invoke(pct, $"Query phase {i}/{totalSteps}...");
        }
        return JsonSerializer.Serialize(new { message = "Query executed", rows = 42, jobId = job.Id });
    }
}
