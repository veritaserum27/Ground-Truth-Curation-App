using Microsoft.AspNetCore.Mvc;
using GroundTruthCuration.Core.Services;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;

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

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<GroundTruthDefinition>>> GetDefinitions([FromQuery] GroundTruthDefinitionFilter? filter = null)
    {
        try
        {
            var definitions = await _groundTruthCurationService.GetAllGroundTruthDefinitionsAsync(filter);
            return Ok(definitions);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message); // 400
        }
        catch (Exception ex)
        {
            // Optionally log ex
            return StatusCode(500, $"Internal server error: {ex.Message}"); // 500
        }
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<GroundTruthDefinition>> CreateDefinition([FromBody] CreateDefinitionRequest request)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    [HttpPut("definitions/{id}/validation-status")]
    public async Task<ActionResult<GroundTruthDefinition>> UpdateValidationStatus(
        Guid id,
        [FromBody] UpdateValidationStatusRequest request)
    {
        throw new NotImplementedException();
    }
}


// Request DTOs
public record CreateDefinitionRequest(string UserQuery, string UserId);
public record CreateEntryRequest(string Response, string RequiredValuesJson, string RawDataJson);
public record UpdateValidationStatusRequest(string ValidationStatus, string UserId);
