using System.ComponentModel.DataAnnotations;
using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Models;

/// <summary>
/// Request payload for submitting a new background job.
/// </summary>
public class SubmitJobRequest
{
    /// <summary>
    /// The job type to execute.
    /// </summary>
    [Required]
    public BackgroundJobType Type { get; set; }
}
