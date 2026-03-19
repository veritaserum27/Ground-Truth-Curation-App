using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Interfaces;

/// <summary>
/// Repository abstraction for managing Tag entities.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Adds a new tag to the datastore.
    /// </summary>
    /// <param name="tag">Tag entity to add.</param>
    /// <returns>The created tag (with identifier).</returns>
    Task<Tag> AddTagAsync(Tag tag);

    /// <summary>
    /// Retrieves a tag by its (case-insensitive) name.
    /// </summary>
    /// <param name="name">Name of the tag.</param>
    /// <returns>The tag if found; otherwise null.</returns>
    Task<Tag?> GetTagByNameAsync(string name);

    /// <summary>
    /// Retrieves all tags.
    /// </summary>
    Task<IEnumerable<Tag>> GetAllTagsAsync();

    /// <summary>
    /// Retrieves a tag by its identifier.
    /// </summary>
    Task<Tag?> GetTagByIdAsync(Guid id);

    /// <summary>
    /// Updates an existing tag.
    /// </summary>
    /// <param name="tag">Tag with updated fields.</param>
    /// <returns>True if an update occurred; otherwise false.</returns>
    Task<bool> UpdateTagAsync(Tag tag);

    /// <summary>
    /// Deletes a tag by id.
    /// </summary>
    /// <returns>True if a row was deleted.</returns>
    Task<bool> DeleteTagAsync(Guid id);
}
