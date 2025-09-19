using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Processing;

/// <summary>
/// Executes a background job and returns its result payload (JSON string) if successful.
/// Implementations may update job progress via the supplied callback.
/// </summary>
public interface IBackgroundJobExecutor
{
    /// <summary>
    /// The job type identifier this executor supports.
    /// </summary>
    string BackgroundJobType { get; }

    /// <summary>
    /// Executes the supplied job.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <param name="progressCallback">Optional callback receiving percentage (0-100) and message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Serialized result payload or null if none.</returns>
    Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken);
}
