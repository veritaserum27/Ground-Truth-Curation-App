using GroundTruthCuration.Jobs.Entities;
using GroundTruthCuration.Jobs.Services;
using System.Text.Json;

namespace GroundTruthCuration.Jobs.Processing.Executors;

public class ExportJobExecutor : IBackgroundJobExecutor
{
    public string SupportedType => "Export";

    public async Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken)
    {
        var totalSteps = 20;
        for (int i = 1; i <= totalSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1_000, cancellationToken).ConfigureAwait(false);
            var pct = (int)Math.Round(i * 100.0 / totalSteps);
            progressCallback?.Invoke(pct, $"Export step {i} of {totalSteps}...");
        }
        return JsonSerializer.Serialize(new { message = "Export completed", jobId = job.Id });
    }
}
