namespace GroundTruthCuration.Core.Entities;

/// <summary>
/// Represents the lifecycle status of a background job.
/// </summary>
public enum BackgroundJobStatus
{
    /// <summary>
    /// The job has been accepted and is waiting to be processed.
    /// </summary>
    Queued = 0,
    /// <summary>
    /// The job is currently being executed.
    /// </summary>
    Running = 1,
    /// <summary>
    /// The job completed successfully and produced a result.
    /// </summary>
    Succeeded = 2,
    /// <summary>
    /// The job failed during execution.
    /// </summary>
    Failed = 3,
    /// <summary>
    /// The job was canceled before completion.
    /// </summary>
    Canceled = 4
}
