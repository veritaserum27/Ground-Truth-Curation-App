using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Core.Interfaces;

/// <summary>
/// Domain service abstraction for Tag operations.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Creates a new tag ensuring validation and uniqueness.
    /// </summary>
    /// <param name="tagDto">Tag data transfer object.</param>
    /// <returns>The created tag DTO.</returns>
    Task<TagDto> CreateTagAsync(TagDto tagDto);

    /// <summary>
    /// Returns all tags.
    /// </summary>
    Task<IEnumerable<TagDto>> GetAllTagsAsync();

    /// <summary>
    /// Returns a tag by id or null.
    /// </summary>
    Task<TagDto?> GetTagByIdAsync(Guid id);

    /// <summary>
    /// Updates an existing tag.
    /// </summary>
    Task<TagDto> UpdateTagAsync(Guid id, TagDto tagDto);

    /// <summary>
    /// Deletes a tag by id.
    /// </summary>
    Task<bool> DeleteTagAsync(Guid id);
}
