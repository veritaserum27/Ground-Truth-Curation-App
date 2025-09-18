using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation(ex, "Conflict creating tag");
            return Conflict(ex.Message);
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
}

public record CreateTagRequest(string Name, string? Description);
