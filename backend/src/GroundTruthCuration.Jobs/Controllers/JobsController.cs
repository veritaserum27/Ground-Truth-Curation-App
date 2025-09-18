using GroundTruthCuration.Api.Models.Jobs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroundTruthCuration.Api.Controllers;

/// <summary>Provides endpoints for submitting and managing background jobs.</summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobService _jobService;

    /// <summary>
    /// Creates a new <see cref="JobsController"/>.
    /// </summary>
    public JobsController(IBackgroundJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>Submits a new background job of the specified type.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BackgroundJobResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Submit([FromBody] SubmitJobRequest request, CancellationToken cancellationToken)
    {
        var job = await _jobService.SubmitJobAsync(request.Type, cancellationToken).ConfigureAwait(false);
        var response = BackgroundJobResponse.FromEntity(job);
        return AcceptedAtAction(nameof(GetJob), new { id = job.Id }, response);
    }

    /// <summary>Retrieves the metadata for a specific job.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BackgroundJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobAsync(id, cancellationToken).ConfigureAwait(false);
        if (job == null)
        {
            return NotFound();
        }
        return Ok(BackgroundJobResponse.FromEntity(job));
    }

    /// <summary>Lists jobs optionally filtered by status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BackgroundJobResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] BackgroundJobStatus[]? status, CancellationToken cancellationToken)
    {
        var jobs = await _jobService.ListJobsAsync(status, cancellationToken).ConfigureAwait(false);
        return Ok(jobs.Select(BackgroundJobResponse.FromEntity));
    }

    /// <summary>Attempts to cancel a queued job.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobAsync(id, cancellationToken).ConfigureAwait(false);
        if (job == null)
        {
            return NotFound();
        }
        if (job.Status != BackgroundJobStatus.Queued)
        {
            return Conflict(new { message = "Job cannot be canceled in its current state." });
        }
        var canceled = await _jobService.CancelJobAsync(id, cancellationToken).ConfigureAwait(false);
        if (!canceled)
        {
            return Conflict(new { message = "Job cancellation failed due to race condition." });
        }
        return NoContent();
    }

    /// <summary>Retrieves the result of a completed job, returns 202 if still processing, or 404 if not found/terminal without result.</summary>
    [HttpGet("{id:guid}/result")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResult(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobAsync(id, cancellationToken).ConfigureAwait(false);
        if (job == null)
        {
            return NotFound();
        }
        if (job.Status == BackgroundJobStatus.Succeeded)
        {
            var result = await _jobService.GetJobResultAsync(id, cancellationToken).ConfigureAwait(false);
            return Ok(new { jobId = id, result });
        }
        if (job.Status is BackgroundJobStatus.Failed or BackgroundJobStatus.Canceled)
        {
            return NotFound();
        }
        return StatusCode(StatusCodes.Status202Accepted, new { message = "Result not ready" });
    }
}
