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
}
