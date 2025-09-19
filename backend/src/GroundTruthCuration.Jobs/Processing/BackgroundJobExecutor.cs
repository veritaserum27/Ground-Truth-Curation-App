using GroundTruthCuration.Jobs.Entities;
using GroundTruthCuration.Jobs.Services;

namespace GroundTruthCuration.Jobs.Processing;

// This obsolete placeholder remains only because deletion via tooling failed during refactor.
// It is no longer registered in DI. New per-type executors implement IBackgroundJobExecutor.
// If encountered, it will throw to indicate misconfiguration.
public class BackgroundJobExecutor : IBackgroundJobExecutor
{
    public string SupportedType => "__obsolete__";
    public Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken)
        => throw new NotSupportedException("BackgroundJobExecutor is obsolete; use per-type executors.");
}
