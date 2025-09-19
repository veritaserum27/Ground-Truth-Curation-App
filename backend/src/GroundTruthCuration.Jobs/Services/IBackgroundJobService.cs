using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Services;

/// <summary>
/// High-level service for submitting and managing background jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Submits a new background job of the specified type and returns the created job.
    /// </summary>
    Task<BackgroundJob> SubmitJobAsync(string type, CancellationToken cancellationToken = default);
    /// <summary>
    /// Submits a new background job of the specified id and type and returns the created job.
    /// </summary>
    Task<BackgroundJob> SubmitJobAsync(Guid id, string type, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves a job by identifier.
    /// </summary>
    Task<BackgroundJob?> GetJobAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists jobs optionally filtered by statuses.
    /// </summary>
    Task<IReadOnlyList<BackgroundJob>> ListJobsAsync(IEnumerable<BackgroundJobStatus>? statuses = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Attempts to cancel a job that has not yet started executing.
    /// Returns true if cancellation was applied.
    /// </summary>
    Task<bool> CancelJobAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves the result data of a completed job or null if not available.
    /// </summary>
    Task<string?> GetJobResultAsync(Guid id, CancellationToken cancellationToken = default);
}
