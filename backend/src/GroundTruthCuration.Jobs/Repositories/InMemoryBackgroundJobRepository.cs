using System.Collections.Concurrent;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Thread-safe in-memory storage of background jobs. Intended for initial development and testing only.
/// </summary>
public class InMemoryBackgroundJobRepository : IBackgroundJobRepository
{
    private readonly ConcurrentDictionary<Guid, BackgroundJob> _jobs = new();

    /// <inheritdoc />
    public Task AddAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<BackgroundJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BackgroundJob>> ListAsync(IEnumerable<BackgroundJobStatus>? statuses = null, CancellationToken cancellationToken = default)
    {
        if (statuses == null)
        {
            return Task.FromResult((IReadOnlyList<BackgroundJob>)_jobs.Values.OrderBy(j => j.CreatedAt).ToList());
        }
        var set = new HashSet<BackgroundJobStatus>(statuses);
        var filtered = _jobs.Values.Where(j => set.Contains(j.Status)).OrderBy(j => j.CreatedAt).ToList();
        return Task.FromResult((IReadOnlyList<BackgroundJob>)filtered);
    }

    /// <inheritdoc />
    public Task UpdateAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        // Replace existing reference (fields already mutated by caller) for completeness.
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> TryUpdateStatusAsync(Guid id, BackgroundJobStatus fromStatus, BackgroundJobStatus toStatus, Action<BackgroundJob>? mutate = null, CancellationToken cancellationToken = default)
    {
        if (!_jobs.TryGetValue(id, out var existing))
        {
            return Task.FromResult(false);
        }
        if (existing.Status != fromStatus)
        {
            return Task.FromResult(false);
        }
        // Clone not required since BackgroundJob is mutable but we rely on atomic dictionary operation below.
        var updated = existing;
        mutate?.Invoke(updated);
        updated.Status = toStatus;
        updated.UpdatedAt = DateTime.UtcNow;

        var success = _jobs.TryUpdate(id, updated, existing);
        return Task.FromResult(success);
    }
}
