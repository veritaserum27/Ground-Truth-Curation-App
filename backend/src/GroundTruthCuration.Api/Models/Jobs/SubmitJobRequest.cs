using System.ComponentModel.DataAnnotations;
using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Api.Models.Jobs;

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
