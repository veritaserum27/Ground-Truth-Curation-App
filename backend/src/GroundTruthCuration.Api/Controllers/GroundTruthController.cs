using Microsoft.AspNetCore.Mvc;
using GroundTruthCuration.Core.Services;
using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroundTruthController : ControllerBase
{
    private readonly IGroundTruthCurationService _groundTruthCurationService;

    public GroundTruthController(IGroundTruthCurationService groundTruthCurationService)
    {
        _groundTruthCurationService = groundTruthCurationService;
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<GroundTruthDefinition>> CreateDefinition([FromBody] CreateDefinitionRequest request)
    {
        try
        {
            var groundTruthDefinition = await _groundTruthCurationService.CreateGroundTruthDefinitionAsync(
                request.UserQuery,
                request.UserId);

            return CreatedAtAction(
                nameof(GetDefinition),
                new { id = groundTruthDefinition.GroundTruthId },
                groundTruthDefinition);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating ground truth definition: {ex.Message}");
        }
    }

    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<GroundTruthDefinition>> GetDefinition(Guid id)
    {
        try
        {
            // This demonstrates the flow: API -> Core Service -> Infrastructure Repository
            var definition = await _groundTruthCurationService.GetGroundTruthDefinitionByIdAsync(id);

            if (definition == null)
            {
                return NotFound($"Ground truth definition with ID {id} not found.");
            }

            return Ok(definition);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving ground truth definition: {ex.Message}");
        }
    }

    [HttpPost("definitions/{id}/entries")]
    public async Task<ActionResult<GroundTruthEntry>> AddEntry(Guid id, [FromBody] CreateEntryRequest request)
    {
        try
        {
            var groundTruthEntry = await _groundTruthCurationService.AddGroundTruthEntryAsync(
                id,
                request.Response,
                request.RequiredValuesJson,
                request.RawDataJson);

            return Created($"api/groundtruth/entries/{groundTruthEntry.GroundTruthEntryId}", groundTruthEntry);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error adding ground truth entry: {ex.Message}");
        }
    }

    [HttpPut("definitions/{id}/validation-status")]
    public async Task<ActionResult<GroundTruthDefinition>> UpdateValidationStatus(
        Guid id,
        [FromBody] UpdateValidationStatusRequest request)
    {
        try
        {
            var updatedDefinition = await _groundTruthCurationService.UpdateValidationStatusAsync(
                id,
                request.ValidationStatus,
                request.UserId);

            return Ok(updatedDefinition);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating validation status: {ex.Message}");
        }
    }
}

// Request DTOs
public record CreateDefinitionRequest(string UserQuery, string UserId);
public record CreateEntryRequest(string Response, string RequiredValuesJson, string RawDataJson);
public record UpdateValidationStatusRequest(string ValidationStatus, string UserId);
