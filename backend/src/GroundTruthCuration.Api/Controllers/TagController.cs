using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GroundTruthCuration.Core.Exceptions;

namespace GroundTruthCuration.Api.Controllers;

/// <summary>
/// API controller for Tag management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagController> _logger;

    public TagController(ITagService tagService, ILogger<TagController> logger)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="request">Tag creation request.</param>
    /// <returns>Created tag DTO with 201 status.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        try
        {
            var created = await _tagService.CreateTagAsync(new TagDto
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty
            });
            return CreatedAtAction(nameof(GetTagByName), new { name = created.Name }, created);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tag");
            return BadRequest(ex.Message);
        }
        catch (DuplicateTagException dup)
        {
            // Return standardized ProblemDetails for 409
            var problem = new ProblemDetails
            {
                Title = "Conflict",
                Detail = dup.Message,
                Status = StatusCodes.Status409Conflict,
                Type = "https://httpstatuses.com/409"
            };
            return Conflict(problem);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation(ex, "Conflict creating tag");
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Type = "https://httpstatuses.com/409"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating tag");
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// Retrieves a tag by name (exact match, case-insensitive).
    /// </summary>
    /// <param name="tagRepository">Injected tag repository (resolved from DI via FromServices).</param>
    /// <param name="name">Tag name (case-insensitive).</param>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagDto>> GetTagByName([FromServices] ITagRepository tagRepository, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name required.");
        var tag = await tagRepository.GetTagByNameAsync(name.Trim());
        if (tag == null) return NotFound();
        return Ok(new TagDto { TagId = tag.TagId, Name = tag.Name, Description = tag.Description });
    }

    /// <summary>
    /// Retrieves all tags.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Updates a tag by id.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagDto>> Update(Guid id, [FromBody] UpdateTagRequest request)
    {
        if (request is null) return BadRequest("Request body required");
        try
        {
            var updated = await _tagService.UpdateTagAsync(id, new TagDto { TagId = id, Name = request.Name, Description = request.Description ?? string.Empty });
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (GroundTruthCuration.Core.Exceptions.DuplicateTagException dup)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = dup.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = ex.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    /// <summary>
    /// Deletes a tag by id.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _tagService.DeleteTagAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public record CreateTagRequest(string Name, string? Description);
public record UpdateTagRequest(string Name, string? Description);
