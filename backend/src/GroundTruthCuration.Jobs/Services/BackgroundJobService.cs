using GroundTruthCuration.Jobs.Entities;
using GroundTruthCuration.Jobs.Queues;
using GroundTruthCuration.Jobs.Repositories;

namespace GroundTruthCuration.Jobs.Services;

/// <summary>
/// Default implementation of <see cref="IBackgroundJobService"/> coordinating repository and queue.
/// </summary>
public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobRepository _repository;
    private readonly IBackgroundJobQueue _queue;

    /// <summary>
    /// Creates a new <see cref="BackgroundJobService"/> instance.
    /// </summary>
    public BackgroundJobService(IBackgroundJobRepository repository, IBackgroundJobQueue queue)
    {
        _repository = repository;
        _queue = queue;
    }

    /// <inheritdoc />
    public async Task<BackgroundJob> SubmitJobAsync(BackgroundJobType type, CancellationToken cancellationToken = default)
    {
        var job = new BackgroundJob
        {
            Type = type,
            Status = BackgroundJobStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await _queue.EnqueueAsync(job, cancellationToken).ConfigureAwait(false);
        return job;
    }

    /// <inheritdoc />
    public Task<BackgroundJob?> GetJobAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<BackgroundJob>> ListJobsAsync(IEnumerable<BackgroundJobStatus>? statuses = null, CancellationToken cancellationToken = default)
        => _repository.ListAsync(statuses, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> CancelJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Only allow cancellation if still queued.
        var success = await _repository.TryUpdateStatusAsync(id, BackgroundJobStatus.Queued, BackgroundJobStatus.Canceled, job =>
        {
            job.CanceledAt = DateTime.UtcNow;
            job.StatusMessage = "Canceled before execution";
        }, cancellationToken).ConfigureAwait(false);
        return success;
    }

    /// <inheritdoc />
    public async Task<string?> GetJobResultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (job == null)
        {
            return null;
        }
        if (job.Status == BackgroundJobStatus.Succeeded)
        {
            return job.ResultData;
        }
        return null;
    }
}
