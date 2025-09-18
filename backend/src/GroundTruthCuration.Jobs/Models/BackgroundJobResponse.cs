using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Models;

/// <summary>
/// Standard response shape for background job metadata.
/// </summary>
public class BackgroundJobResponse
{
    /// <summary>Job identifier.</summary>
    public Guid Id { get; set; }
    /// <summary>Type of work performed.</summary>
    public BackgroundJobType Type { get; set; }
    /// <summary>Current lifecycle status.</summary>
    public BackgroundJobStatus Status { get; set; }
    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Execution start timestamp (UTC) if applicable.</summary>
    public DateTime? StartedAt { get; set; }
    /// <summary>Completion timestamp (UTC) if succeeded or failed.</summary>
    public DateTime? CompletedAt { get; set; }
    /// <summary>Cancellation timestamp (UTC) if canceled.</summary>
    public DateTime? CanceledAt { get; set; }
    /// <summary>Progress percentage (0-100) if reported.</summary>
    public int? Progress { get; set; }
    /// <summary>Human-readable status message.</summary>
    public string? StatusMessage { get; set; }
    /// <summary>Error details if failed.</summary>
    public string? Error { get; set; }

    /// <summary>
    /// Maps a <see cref="BackgroundJob"/> entity to a response DTO.
    /// </summary>
    public static BackgroundJobResponse FromEntity(BackgroundJob job) => new()
    {
        Id = job.Id,
        Type = job.Type,
        Status = job.Status,
        CreatedAt = job.CreatedAt,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt,
        CanceledAt = job.CanceledAt,
        Progress = job.Progress,
        StatusMessage = job.StatusMessage,
        Error = job.Error
    };
}
