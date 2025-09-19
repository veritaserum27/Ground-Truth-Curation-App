namespace GroundTruthCuration.Jobs.Entities;

/// <summary>
/// Represents a unit of asynchronous background work tracked by the system.
/// </summary>
public class BackgroundJob
{
    /// <summary>
    /// Unique identifier for the job.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// The job's functional type identifier which determines how it is executed.
    /// Stored as a non-empty string.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Current lifecycle status of the job.
    /// </summary>
    public BackgroundJobStatus Status { get; set; } = BackgroundJobStatus.Queued;
    /// <summary>
    /// UTC timestamp when the job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the last update to the job metadata.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>
    /// UTC timestamp when execution started.
    /// </summary>
    public DateTime? StartedAt { get; set; }
    /// <summary>
    /// UTC timestamp when execution completed successfully or failed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    /// <summary>
    /// UTC timestamp when the job was canceled (if applicable).
    /// </summary>
    public DateTime? CanceledAt { get; set; }
    /// <summary>
    /// Optional integer progress percentage (0-100) for long-running jobs.
    /// </summary>
    public int? Progress { get; set; }
    /// <summary>
    /// Human-readable status message or progress detail.
    /// </summary>
    public string? StatusMessage { get; set; }
    /// <summary>
    /// Reference to externally stored result data (e.g., file path) if applicable.
    /// </summary>
    public string? ResultReference { get; set; }
    /// <summary>
    /// Inline serialized result payload (JSON) for small result sets.
    /// </summary>
    public string? ResultData { get; set; }
    /// <summary>
    /// Error details if the job failed.
    /// </summary>
    public string? Error { get; set; }
}
