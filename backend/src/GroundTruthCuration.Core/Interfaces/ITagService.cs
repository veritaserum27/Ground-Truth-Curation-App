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
}
