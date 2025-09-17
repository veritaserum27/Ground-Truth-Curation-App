using Microsoft.AspNetCore.Mvc;
using GroundTruthCuration.Core.Services;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Api.Controllers;


/// <summary>
/// API controller for managing ground truth curation operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroundTruthController : ControllerBase
{
    private readonly ILogger<GroundTruthController> _logger;
    private readonly IGroundTruthCurationService _groundTruthCurationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroundTruthController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="groundTruthCurationService">The service for managing ground truth curation operations.</param>
    public GroundTruthController(ILogger<GroundTruthController> logger, IGroundTruthCurationService groundTruthCurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _groundTruthCurationService = groundTruthCurationService ?? throw new ArgumentNullException(nameof(groundTruthCurationService));
    }

    /// <summary>
    /// Retrieves a list of ground truth definitions based on the provided filter. If no filter is provided, returns all.
    /// </summary>
    /// <param name="filter">Optional filter object containing query parameters for retrieving ground truth definitions (e.g., ValidationStatus, UserQuery).</param>
    /// <returns>A list of ground truth definitions matching the filter criteria.</returns>
    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<GroundTruthDefinition>>> GetDefinitions([FromQuery] GroundTruthDefinitionFilter? filter = null)
    {
        _logger.LogInformation("Fetching ground truth definitions with filter: {@Filter}", filter);
        try
        {
            var definitions = await _groundTruthCurationService.GetAllGroundTruthDefinitionsAsync(filter);
            return Ok(definitions);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error fetching ground truth definitions");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching ground truth definitions");
            return StatusCode(500, $"Internal server error: {ex.Message}"); // 500
        }
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<GroundTruthDefinition>> CreateDefinition([FromBody] CreateDefinitionRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves a specific ground truth definition by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    /// <returns>The ground truth definition if found, or a NotFound result if not.</returns>
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
