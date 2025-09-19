using System.Text.Json;
using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Processing.Executors;

public class ResponseGenerationJobExecutor : IBackgroundJobExecutor
{
    public string BackgroundJobType => "ResponseGeneration";

    public async Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken)
    {
        var totalSteps = 15;
        for (int i = 1; i <= totalSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1_000, cancellationToken).ConfigureAwait(false);
            var pct = (int)Math.Round(i * 100.0 / totalSteps);
            progressCallback?.Invoke(pct, $"Generating content {i}/{totalSteps}...");
        }
        return JsonSerializer.Serialize(new { message = "Response generated", jobId = job.Id });
    }
}
