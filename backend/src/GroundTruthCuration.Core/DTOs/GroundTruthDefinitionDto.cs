namespace GroundTruthCuration.Core.DTOs;

public class GroundTruthDefinitionDto
{
    public Guid GroundTruthId { get; set; }
    public string UserQuery { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UserCreated { get; set; } = string.Empty;
    public string? UserUpdated { get; set; }
    public DateTime CreationDateTime { get; set; }
    public ICollection<GroundTruthEntryDto> GroundTruthEntries { get; set; } = new List<GroundTruthEntryDto>();
    public ICollection<DataQueryDefinitionDto> DataQueryDefinitions { get; set; } = new List<DataQueryDefinitionDto>();
    public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
}