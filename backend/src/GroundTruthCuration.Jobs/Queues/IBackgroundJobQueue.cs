using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Queues;

/// <summary>
/// Abstraction for enqueueing and dequeueing background jobs for processing.
/// </summary>
public interface IBackgroundJobQueue
{
    /// <summary>
    /// Enqueues a job for later processing.
    /// </summary>
    Task EnqueueAsync(BackgroundJob job, CancellationToken cancellationToken = default);
    /// <summary>
    /// Dequeues the next available job, awaiting if none are currently queued.
    /// </summary>
    Task<BackgroundJob> DequeueAsync(CancellationToken cancellationToken);
}
