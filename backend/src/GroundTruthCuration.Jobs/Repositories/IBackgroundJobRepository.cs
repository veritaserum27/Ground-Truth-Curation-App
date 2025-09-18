using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Interfaces;

/// <summary>
/// Repository abstraction for persisting and querying background jobs.
/// </summary>
public interface IBackgroundJobRepository
{
    /// <summary>
    /// Adds a new job to the repository.
    /// </summary>
    Task AddAsync(BackgroundJob job, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves a job by its identifier or null if not found.
    /// </summary>
    Task<BackgroundJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists jobs optionally filtering by a set of statuses.
    /// </summary>
    Task<IReadOnlyList<BackgroundJob>> ListAsync(IEnumerable<BackgroundJobStatus>? statuses = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Persists changes to an existing job.
    /// </summary>
    Task UpdateAsync(BackgroundJob job, CancellationToken cancellationToken = default);
    /// <summary>
    /// Attempts to update the job status atomically from an expected prior value.
    /// Returns true if the transition succeeded.
    /// </summary>
    Task<bool> TryUpdateStatusAsync(Guid id, BackgroundJobStatus fromStatus, BackgroundJobStatus toStatus, Action<BackgroundJob>? mutate = null, CancellationToken cancellationToken = default);
}
