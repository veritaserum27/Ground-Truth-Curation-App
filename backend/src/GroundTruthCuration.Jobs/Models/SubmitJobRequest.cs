using System.ComponentModel.DataAnnotations;
using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Models;

/// <summary>
/// Request payload for submitting a new background job.
/// </summary>
public class SubmitJobRequest
{
    /// <summary>
    /// The job type identifier to execute (case-sensitive string).
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Type { get; set; } = string.Empty;
}
