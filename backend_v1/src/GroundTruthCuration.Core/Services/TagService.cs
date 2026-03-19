using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GroundTruthCuration.Core.Services;

/// <summary>
/// Implements domain logic for Tag creation and validation.
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TagService> _logger;

    public TagService(ITagRepository tagRepository, ILogger<TagService> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TagDto> CreateTagAsync(TagDto tagDto)
    {
        if (tagDto is null)
            throw new ArgumentNullException(nameof(tagDto));

        var name = tagDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(tagDto.Name));

        if (name.Length > 100)
            throw new ArgumentException("Tag name must be 100 characters or fewer.", nameof(tagDto.Name));

        // Uniqueness (case-insensitive)
        var existing = await _tagRepository.GetTagByNameAsync(name);
        if (existing != null)
            throw new InvalidOperationException($"A tag with name '{name}' already exists.");

        var entity = new Tag
        {
            TagId = tagDto.TagId == Guid.Empty ? Guid.NewGuid() : tagDto.TagId,
            Name = name,
            Description = tagDto.Description?.Trim() ?? string.Empty
        };

        var created = await _tagRepository.AddTagAsync(entity);
        _logger.LogInformation("Created new tag {TagId} ({Name})", created.TagId, created.Name);

        return new TagDto
        {
            TagId = created.TagId,
            Name = created.Name,
            Description = created.Description
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _tagRepository.GetAllTagsAsync();
        return tags.Select(t => new TagDto { TagId = t.TagId, Name = t.Name, Description = t.Description });
    }

    /// <inheritdoc />
    public async Task<TagDto?> GetTagByIdAsync(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty", nameof(id));
        var tag = await _tagRepository.GetTagByIdAsync(id);
        return tag == null ? null : new TagDto { TagId = tag.TagId, Name = tag.Name, Description = tag.Description };
    }

    /// <inheritdoc />
    public async Task<TagDto> UpdateTagAsync(Guid id, TagDto tagDto)
    {
        if (id == Guid.Empty) throw new ArgumentException("Tag Id cannot be empty", nameof(id));
        if (tagDto is null) throw new ArgumentNullException(nameof(tagDto));

        var existing = await _tagRepository.GetTagByIdAsync(id) ?? throw new KeyNotFoundException($"Tag {id} not found");

        var newName = tagDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Tag name is required", nameof(tagDto.Name));
        if (newName!.Length > 100) throw new ArgumentException("Tag name must be 100 characters or fewer", nameof(tagDto.Name));

        // uniqueness check by name (exclude self)
        var dup = await _tagRepository.GetTagByNameAsync(newName);
        if (dup != null && dup.TagId != id)
            throw new InvalidOperationException($"A tag with name '{newName}' already exists.");

        existing.Name = newName;
        existing.Description = tagDto.Description?.Trim() ?? string.Empty;

        var updated = await _tagRepository.UpdateTagAsync(existing);
        if (!updated)
            throw new InvalidOperationException("Update failed (no rows affected)");

        return new TagDto { TagId = existing.TagId, Name = existing.Name, Description = existing.Description };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTagAsync(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty", nameof(id));
        return await _tagRepository.DeleteTagAsync(id);
    }
}
